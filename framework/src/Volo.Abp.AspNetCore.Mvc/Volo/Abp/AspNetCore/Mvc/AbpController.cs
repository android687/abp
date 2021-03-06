﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Aspects;
using Volo.Abp.AspNetCore.Mvc.Validation;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace Volo.Abp.AspNetCore.Mvc
{
    public abstract class AbpController : Controller, IAvoidDuplicateCrossCuttingConcerns
    {
        public IServiceProvider ServiceProvider { get; set; }
        protected readonly object ServiceProviderLock = new object();
        protected TService LazyGetRequiredService<TService>(ref TService reference)
        {
            if (reference == null)
            {
                lock (ServiceProviderLock)
                {
                    if (reference == null)
                    {
                        reference = ServiceProvider.GetRequiredService<TService>();
                    }
                }
            }

            return reference;
        }

        public IUnitOfWorkManager UnitOfWorkManager => LazyGetRequiredService(ref _unitOfWorkManager);
        private IUnitOfWorkManager _unitOfWorkManager;

        public IObjectMapper ObjectMapper => LazyGetRequiredService(ref _objectMapper);
        private IObjectMapper _objectMapper;

        public IGuidGenerator GuidGenerator => LazyGetRequiredService(ref _guidGenerator);
        private IGuidGenerator _guidGenerator;

        public ILoggerFactory LoggerFactory => LazyGetRequiredService(ref _loggerFactory);
        private ILoggerFactory _loggerFactory;

        public ICurrentUser CurrentUser => LazyGetRequiredService(ref _currentUser);
        private ICurrentUser _currentUser;

        public ICurrentTenant CurrentTenant => LazyGetRequiredService(ref _currentTenant);
        private ICurrentTenant _currentTenant;

        public IAuthorizationService AuthorizationService => LazyGetRequiredService(ref _authorizationService);
        private IAuthorizationService _authorizationService;

        protected IUnitOfWork CurrentUnitOfWork => UnitOfWorkManager?.Current;

        public IClock Clock => LazyGetRequiredService(ref _clock);
        private IClock _clock;

        public IModelStateValidator ModelValidator => LazyGetRequiredService(ref _modelValidator);
        private IModelStateValidator _modelValidator;

        public IFeatureChecker FeatureChecker => LazyGetRequiredService(ref _featureChecker);
        private IFeatureChecker _featureChecker;

        public List<string> AppliedCrossCuttingConcerns { get; } = new List<string>();

        protected virtual void ValidateModel()
        {
            ModelValidator?.Validate(ModelState);
        }

        protected ILogger Logger => _lazyLogger.Value;
        private Lazy<ILogger> _lazyLogger => new Lazy<ILogger>(() => LoggerFactory?.CreateLogger(GetType().FullName) ?? NullLogger.Instance, true);
    }
}
