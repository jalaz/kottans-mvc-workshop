using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Mvc;
using Workshop.Controllers;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Workshop.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Workshop.App_Start.NinjectWebCommon), "Stop")]

namespace Workshop.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            AssemblyScanner.FindValidatorsInAssemblyContaining<ImportantClient>()
                .ForEach(a =>
                {
                    kernel.Bind(a.InterfaceType).To(a.ValidatorType);
                });

            FluentValidationModelValidatorProvider.Configure(provider =>
            {
                provider.ValidatorFactory = new NinjectValidatorFactory(kernel);
            });

            kernel.Bind<IClientRepository>().To<InMemoryClientRepository>().InSingletonScope();
        }
    }

    internal class NinjectValidatorFactory : IValidatorFactory
    {
        private readonly IKernel kernel;

        public NinjectValidatorFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IValidator<T> GetValidator<T>()
        {
            return kernel.Get<IValidator<T>>();
        }

        public IValidator GetValidator(Type type)
        {
            return kernel.Get(type) as IValidator;
        }
    }

    public class InMemoryClientRepository : IClientRepository
    {
        private readonly List<ImportantClient> clients = new List<ImportantClient>
        {
            new ImportantClient {Id = Guid.NewGuid(), FirstName = "Vasya", SecondName = "Pupkin"},
            new ImportantClient {Id = Guid.NewGuid(), FirstName = "Jora", SecondName = "Veselkin"}
        };

        public List<ImportantClient> GetAll()
        {
            return clients;
        }

        public ImportantClient GetById(Guid id)
        {
            return clients.FirstOrDefault(c => c.Id == id);
        }

        public void Add(ImportantClient client)
        {
            client.Id = Guid.NewGuid();
            clients.Add(client);
        }

        public void Update(ImportantClient client)
        {
            var clientIndex = clients.FindIndex(c => c.Id == client.Id);
            clients[clientIndex] = client;
        }
    }

    public interface IClientRepository
    {
        List<ImportantClient> GetAll();
        ImportantClient GetById(Guid id);
        void Add(ImportantClient client);
        void Update(ImportantClient client);
    }
}
