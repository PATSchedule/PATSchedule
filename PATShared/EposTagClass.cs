using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PATShared
{
    public class EposTagClass
    {
        public AntiEpos.AEProgressData[] Progress;

        public static async Task EnsureEposAuth(AntiEpos.AEClient epos, string rsaagLogin, string rsaagPassword)
        {
            await epos.Login(rsaagLogin, rsaagPassword);
            await epos.CheckAgreement();
            // если отработал CheckAgreement() то и Authorize должен без ошибок пройти
        }
    }
}
