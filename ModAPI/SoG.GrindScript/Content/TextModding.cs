﻿using System;
using SoG.Modding.Core;

namespace SoG.Modding.Content
{
    public class TextModding : ModdingLogic
    {
        public TextModding(GrindScript modAPI)
            : base(modAPI) { }

        public void AddMiscText(string sCategory, string sEntry, string sText, MiscTextTypes enType = MiscTextTypes.Default)
        {
            try
            {
                MiscText xEntry = new MiscText();
                _modAPI.Game.xMiscTextGod_Default.dsxTextCollections[sCategory].dsxTexts[sEntry] = xEntry;

                // No support for color tags yet...
                xEntry.sUnparsedFullLine = xEntry.sUnparsedBaseLine = sText;

                switch (enType)
                {
                    case MiscTextTypes.GenericSpellMoreInfo:
                        // This is where extra spell info is written
                        xEntry.iMaxWidth = 350;
                        xEntry.iMaxHeight = 164;
                        xEntry.enFontType = FontManager.FontType.Reg7;
                        break;
                    case MiscTextTypes.GenericSpellName:
                    case MiscTextTypes.GenericItemName:
                        // Talent names also fit here
                        xEntry.iMaxWidth = 170;
                        xEntry.iMaxHeight = 19;
                        xEntry.enFontType = FontManager.FontType.Bold8Spacing1;
                        break;
                    case MiscTextTypes.GenericSpellFlavor:
                    case MiscTextTypes.GenericItemDescription:
                        // Talent details also fit here
                        xEntry.iMaxWidth = 280;
                        xEntry.iMaxHeight = 30;
                        xEntry.enFontType = FontManager.FontType.Reg7;
                        break;
                    case MiscTextTypes.Default:
                    default:
                        // Failsafe case
                        xEntry.iMaxWidth = 100;
                        xEntry.iMaxHeight = 100;
                        xEntry.enFontType = FontManager.FontType.Reg7;
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't add misc text " + sCategory + ":" + sEntry + ":" + sText);
                Console.WriteLine("Reason: " + e);
            }
        }

        public string GetMiscText(string sCategory, string sEntry)
        {
            try
            {
                // try-catch addiction
                return _modAPI.Game.xMiscTextGod_Default.dsxTextCollections[sCategory].dsxTexts[sEntry].sUnparsedBaseLine;
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't properly retrieve misc text " + sCategory + ":" + sEntry);
                Console.WriteLine("Reason: " + e);
                return "Yo, string not found!";
            }
        }
    }
}
