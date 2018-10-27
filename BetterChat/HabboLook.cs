using Sulakore.Habbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BetterChat
{
    public static class HabboLook
    {
        public const string S_3_G_1_D_2_H_2_A_3_F_0 = "s-3.g-1.d-2.h-2.a-3.f-0";
	    public const string S_2_G_1_D_4_H_4_A_3_F_0 = "s-2.g-1.d-4.h-4.a-3.f-0";
	    public const string S_2_G_1_D_2_H_2_A_3_F_0 = "s-2.g-1.d-2.h-2.a-3.f-0";
	    public const string S_0_G_1_D_3_H_3_A_3_F_0 = "s-0.g-1.d-3.h-3.a-3.f-0";
	    public const string S_0_G_1_D_2_H_2_A_3_F_0 = "s-0.g-1.d-2.h-2.a-3.f-0";
	    public const string S_0_G_0_D_2_H_2_A_0_F_0 = "s-0.g-0.d-2.h-2.a-0.f-0";
        public const string S_2_G_1_D_2_H_2_A_0 = "s-2.g-1.d-2.h-2.a-0";
        private const string IMAGER_SALT = "ef2356a4926bf225eb86c75c52309c32";

        private static string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }

        public static string GetImagingUrl(string figureString, HHotel hotel, string gesture = S_2_G_1_D_2_H_2_A_0)
            => $"https://www.habbo.{hotel.ToDomain()}/habbo-imaging/avatar/{figureString},{gesture},{CalculateMD5Hash(figureString + gesture + IMAGER_SALT)}.png";
        
    }
}
