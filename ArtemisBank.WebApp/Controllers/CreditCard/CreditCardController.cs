using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBank.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CreditCardController : Controller
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ICreditCardConsumptionService _consumptionService;
        private readonly IAccountServiceForWebApp _accountService;
        private readonly ILoanService _loanService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CreditCardController(
            ICreditCardService creditCardService,
            ICreditCardConsumptionService consumptionService,
            IAccountServiceForWebApp accountService,
            ILoanService loanService,
            IEmailService emailService,
            IMapper mapper)
        {
            _creditCardService = creditCardService;
            _consumptionService = consumptionService;
            _accountService = accountService;
            _loanService = loanService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var cards = await _creditCardService.GetAllList();
            var users = await _accountService.GetAllUsersAsync();

            var page = int.TryParse(Request.Query["page"], out int p) ? p : 1;
            var statusFilter = Request.Query["status"].ToString();
            var searchIdCard = Request.Query["idcard"].ToString();

            var filteredCards = cards.AsQueryable();

            if (!string.IsNullOrEmpty(searchIdCard))
            {
                var user = users.FirstOrDefault(u => u.IdCard == searchIdCard);
                if (user != null)
                    filteredCards = filteredCards.Where(c => c.UserId == user.Id);
            }

            if (statusFilter == "activas")
                filteredCards = filteredCards.Where(c => c.IsActive);
            else if (statusFilter == "canceladas")
                filteredCards = filteredCards.Where(c => !c.IsActive);

            var cardList = filteredCards.OrderByDescending(c => c.CreatedAt).ToList();

            var pageSize = 20;
            var totalPages = (int)Math.Ceiling(cardList.Count / (double)pageSize);
            var pagedCards = cardList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = pagedCards.Select(c => new CreditCardViewModel
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                CreditLimit = c.CreditLimit,
                AmountOwed = c.AmountOwed,
                ExpirationDate = c.ExpirationDate,
                IsActive = c.IsActive,
                UserId = c.UserId,
                OwnerFullName = users.FirstOrDefault(u => u.Id == c.UserId)?.FirstName + " " +
                                users.FirstOrDefault(u => u.Id == c.UserId)?.LastName ?? ""
            }).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchIdCard = searchIdCard;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AssignCard()
        {
            var users = await _accountService.GetAllUsersAsync();
            var loans = await _loanService.GetAllList();
            var cards = await _creditCardService.GetAllList();

            var activeClients = users.Where(u => u.UserType == "Cliente" && u.IsActive).ToList();

            decimal totalDebt = loans.Where(l => l.IsActive).Sum(l => l.Amount) +
                                cards.Where(c => c.IsActive).Sum(c => c.AmountOwed);
            decimal averageDebt = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0;

            var vm = new AssignCreditCardViewModel
            {
                AverageDebt = averageDebt,
                Clients = activeClients.Select(u => new ClientListViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IdCard = u.IdCard,
                    Email = u.Email,
                    TotalDebt = loans.Where(l => l.UserId == u.Id).Sum(l => l.Amount) +
                                cards.Where(c => c.UserId == u.Id).Sum(c => c.AmountOwed)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                return RedirectToAction("AssignCard");

            var user = await _accountService.GetUserByIdAsync(clientId);
            if (user == null)
                return RedirectToAction("AssignCard");

            var vm = new CreditCardFormViewModel
            {
                ClientId = clientId,
                ClientName = $"{user.FirstName} {user.LastName}"
            };

            return View("CardForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCard(CreditCardFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("CardForm", vm);

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            // Generar número único de 16 dígitos
            var random = new Random();
            string cardNumber;
            var allCards = await _creditCardService.GetAllList();
            do
            {
                cardNumber = string.Concat(Enumerable.Range(0, 4).Select(_ => random.Next(1000, 9999).ToString()));
            } while (allCards.Any(c => c.CardNumber == cardNumber));

            // Generar CVC y cifrarlo con SHA256
            string cvc = random.Next(100, 999).ToString();
            string hashedCvc = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cvc)));

            // Fecha de expiración: 3 años a partir de hoy en formato MM/yy
            string expirationDate = DateTime.Now.AddYears(3).ToString("MM/yy");

            var cardDto = new CreditCardDto
            {
                CardNumber = cardNumber,
                CreditLimit = vm.CreditLimit,
                AmountOwed = 0,
                ExpirationDate = expirationDate,
                CVC = hashedCvc,
                IsActive = true,
                UserId = vm.ClientId,
                AdminId = adminId,
                CreatedAt = DateTime.Now
            };

            await _creditCardService.AddAsync(cardDto);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var card = await _creditCardService.GetById(id);
            if (card == null) return RedirectToAction("Index");

            var consumptions = await _consumptionService.GetAllList();
            card.Consumptions = consumptions
                .Where(c => c.CreditCardId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(card);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _creditCardService.GetById(id);
            if (card == null) return RedirectToAction("Index");

            var vm = new EditCreditCardViewModel
            {
                Id = card.Id,
                CardNumber = card.CardNumber,
                CreditLimit = card.CreditLimit,
                AmountOwed = card.AmountOwed
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditCreditCardViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (vm.CreditLimit < vm.AmountOwed)
            {
                vm.HasError = true;
                vm.Errors.Add("El nuevo límite no puede ser inferior a la deuda actual.");
                return View(vm);
            }

            await _creditCardService.UpdateLimitAsync(vm.Id, vm.CreditLimit);

            var card = await _creditCardService.GetById(vm.Id);
            var user = await _accountService.GetUserByIdAsync(card!.UserId);
            if (user != null)
            {
                string lastFour = vm.CardNumber.Substring(vm.CardNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = "Actualización de límite de tarjeta",
                    HtmlBody = $"El límite de su tarjeta terminada en {lastFour} ha sido modificado. " +
                               $"Nuevo límite: RD$ {vm.CreditLimit:N2}."
                });
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var card = await _creditCardService.GetById(id);
            if (card == null) return RedirectToAction("Index");

            var vm = new CancelCreditCardViewModel
            {
                Id = card.Id,
                CardNumber = card.CardNumber,
                AmountOwed = card.AmountOwed
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCancel(CancelCreditCardViewModel vm)
        {
            var card = await _creditCardService.GetById(vm.Id);
            if (card == null) return RedirectToAction("Index");

            if (card.AmountOwed > 0)
            {
                vm.HasError = true;
                TempData["Error"] = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente.";
                return RedirectToAction("Cancel", new { id = vm.Id });
            }

            await _creditCardService.CancelAsync(vm.Id);
            return RedirectToAction("Index");
        }
    }
}