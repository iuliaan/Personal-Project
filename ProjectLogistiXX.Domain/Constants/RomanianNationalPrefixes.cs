using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Constants
{
    public static class RomanianNationalPrefixes
    {
        public static readonly FrozenDictionary<string, string> Landlines = new Dictionary<string, string>
        {
            {"021", "București"}, {"031", "București"},
            {"0258", "Alba"}, {"0358", "Alba"},
            {"0257", "Arad"}, {"0357", "Arad"},
            {"0248", "Argeș"}, {"0348", "Argeș"},
            {"0234", "Bacău"}, {"0334", "Bacău"},
            {"0259", "Bihor"}, {"0359", "Bihor"},
            {"0263", "Bistrița-Năsăud"}, {"0363", "Bistrița-Năsăud"},
            {"0231", "Botoșani"}, {"0331", "Botoșani"},
            {"0268", "Brașov"}, {"0368", "Brașov"},
            {"0239", "Brăila"}, {"0339", "Brăila"},
            {"0238", "Buzău"}, {"0338", "Buzău"},
            {"0255", "Caraș-Severin"}, {"0355", "Caraș-Severin"},
            {"0242", "Călărași"}, {"0342", "Călărași"},
            {"0264", "Cluj"}, {"0364", "Cluj"},
            {"0241", "Constanța"}, {"0341", "Constanța"},
            {"0267", "Covasna"}, {"0367", "Covasna"},
            {"0245", "Dâmbovița"}, {"0345", "Dâmbovița"},
            {"0251", "Dolj"}, {"0351", "Dolj"},
            {"0236", "Galați"}, {"0336", "Galați"},
            {"0246", "Giurgiu"}, {"0346", "Giurgiu"},
            {"0253", "Gorj"}, {"0353", "Gorj"},
            {"0266", "Harghita"}, {"0366", "Harghita"},
            {"0254", "Hunedoara"}, {"0354", "Hunedoara"},
            {"0243", "Ialomița"}, {"0343", "Ialomița"},
            {"0232", "Iași"}, {"0332", "Iași"},
            {"0279", "Ilfov"}, {"0379", "Ilfov"},
            {"0262", "Maramureș"}, {"0362", "Maramureș"},
            {"0252", "Mehedinți"}, {"0352", "Mehedinți"},
            {"0265", "Mureș"}, {"0365", "Mureș"},
            {"0233", "Neamț"}, {"0333", "Neamț"},
            {"0249", "Olt"}, {"0349", "Olt"},
            {"0244", "Prahova"}, {"0344", "Prahova"},
            {"0261", "Satu Mare"}, {"0361", "Satu Mare"},
            {"0260", "Sălaj"}, {"0360", "Sălaj"},
            {"0269", "Sibiu"}, {"0369", "Sibiu"},
            {"0230", "Suceava"}, {"0330", "Suceava"},
            {"0247", "Teleorman"}, {"0347", "Teleorman"},
            {"0256", "Timiș"}, {"0356", "Timiș"},
            {"0240", "Tulcea"}, {"0340", "Tulcea"},
            {"0235", "Vaslui"}, {"0335", "Vaslui"},
            {"0250", "Vâlcea"}, {"0350", "Vâlcea"},
            {"0237", "Vrancea"}, {"0337", "Vrancea"}

        }.ToFrozenDictionary();
    }
}
