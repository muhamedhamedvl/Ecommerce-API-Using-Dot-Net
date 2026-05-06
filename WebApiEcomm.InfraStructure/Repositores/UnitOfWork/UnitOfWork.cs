using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;
using WebApiEcomm.Core.Interfaces;
using WebApiEcomm.InfraStructure.Data;
using WebApiEcomm.InfraStructure.Repositories;
using AutoMapper;
using WebApiEcomm.Core.Services;
using WebApiEcomm.Core.Interfaces.Auth;

namespace WebApiEcomm.InfraStructure.Repositores.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMapper mapper;
        private readonly IImageManagementService imageManagementService;


        public ICategoryRepository CategoryRepository { get; }

        public IProductRepository ProductRepository { get; }

        public IPhotoRepository PhotoRepository { get; }

        public ICustomerBasketRepository CustomerBasketRepository { get; }

        public IAuth Auth { get; }

        public UnitOfWork(
            AppDbContext context,
            IMapper mapper,
            IImageManagementService imageManagementService,
            ICustomerBasketRepository customerBasketRepository,
            IAuth auth)
        {
            this._context = context;
            this.mapper = mapper;
            this.imageManagementService = imageManagementService;
            CategoryRepository = new CategoryRepository(_context);
            ProductRepository = new ProductRepository(_context, mapper, imageManagementService);
            PhotoRepository = new PhotoRepository(_context);
            CustomerBasketRepository = customerBasketRepository;
            Auth = auth;

        }
    }
}
