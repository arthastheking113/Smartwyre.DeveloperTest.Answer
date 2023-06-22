using Smartwyre.DeveloperTest.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartwyre.DeveloperTest.Data
{
    public interface IRebateDataStore
    {
        public Rebate GetRebate(string rebateIdentifier);

        public void StoreCalculationResult(Rebate account, decimal rebateAmount);
    }
}
