using FlashTextParser.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlashTextParser.Interfaces;
public interface IBannedWordController
{
    Task<JsonResult> GetAllBannedWords();
    Task<JsonResult> GetBannedWord(int idKey, string word);
    Task<JsonResult> AddBannedWord(BannedWord bannedWord);
    Task<JsonResult> UpdateBannedWord(int id, BannedWord bannedWord);
    Task<JsonResult> DeleteBannedWord(int id);

    JsonResult SanitizeText(string textToSanitize);


}
