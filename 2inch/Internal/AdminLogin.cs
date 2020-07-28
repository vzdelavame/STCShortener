using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _2inch.Internal
{
    public class AdminLogin
    {
        public static async Task<bool> CheckCred(Models.Auth auth) //auth should contain Name and Pass
        { //Static je tu zatial len preto, aby mi nevyhadzoval refference error.
            //var TestCheck = true; //TOTO JE LEN EXAMPLE
            //var result = TestCheck; //TOTO JE LEN EXAMPLE
            //Treba tu pridat takyto test:
            //If the Credentials are in the db, result = true, if not, result = false
            return false;
        }
    }
}
