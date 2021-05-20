using CheckListManager.Common.Core.Web.Security;
using CheckListManager.Common.Logic.Api.Messages;
using CheckListManager.Reports.Logic.Detail;
using CheckListManager.Reports.Logic.List;
using CheckListManager.Reports.Tests.Contexts;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace CheckListManager.Reports.Tests.Steps
{
    [Binding]
    public class ReportsListSteps
    {
        private readonly ReportsContext _context;
        private readonly IListService _listService;
        private readonly IDetailService _detailService;
        private readonly IDefaultIdentity _defaultIdentity;

        public ReportsListSteps(
            ReportsContext context,
            IListService listService,
            IDetailService detailService,
            IDefaultIdentity defaultIdentity)
        {
            _context = context;
            _listService = listService;
            _detailService = detailService;
            _defaultIdentity = defaultIdentity;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _context.Request = new DetailRequest();
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _context.Request = null;
        }
    }
}
