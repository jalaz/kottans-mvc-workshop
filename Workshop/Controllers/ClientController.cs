using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using Workshop.App_Start;
using FluentValidation.Attributes;

namespace Workshop.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientRepository clientRepository;

        public ClientController(IClientRepository clientRepository)
        {
            this.clientRepository = clientRepository;
        }

        public ActionResult Index()
        {
            return View(clientRepository.GetAll());
        }

        [HttpGet]
        public ActionResult Detail(Guid? id)
        {
            ImportantClient client = new ImportantClient();
            if (id.HasValue)
            {
                client = clientRepository.GetById(id.Value) ?? new ImportantClient();
            }

            return View(client);
        }

        [HttpPost]
        //[TransactionScope]
        public ActionResult Detail(ImportantClient client)
        {
            if (!ModelState.IsValid)
            {
                return View(client);
            }
            try
            {
                if (client.Id.HasValue)
                {
                    clientRepository.Update(client);
                }
                else
                {
                    clientRepository.Add(client);
                }

            }
            catch (ValidationException)
            {
                
                
            }
            
            
            return RedirectToAction("Index");
        }
    }

    public class ImportantClientValidator : AbstractValidator<ImportantClient>
    {
        public ImportantClientValidator()
        {
            RuleFor(m => m.FirstName).NotEmpty()
                .Must(BeNotLengthy).WithMessage("First Name should not be so lengthy");
        }

        private bool BeNotLengthy(string firstName)
        {
            return firstName.Length < 10;
        }
    }

    [Validator(typeof(ImportantClientValidator))]
    public class ImportantClient
    {
        public Guid? Id { get; set; }
        [Display(Name = "First Name")]
        
        public string FirstName { get; set; }
        [Display(Name = "Second Name")]
        [MyCustomAttribute]
        public string SecondName { get; set; }
    }

    public class MyModelBinder : DefaultModelBinder
    {
        protected override void BindProperty(ControllerContext controllerContext, ModelBindingContext bindingContext,
            PropertyDescriptor propertyDescriptor)
        {

            if (propertyDescriptor.PropertyType == typeof(DateTime))
            {
                
            }
            base.BindProperty(controllerContext, bindingContext, propertyDescriptor);
        }
    }
}