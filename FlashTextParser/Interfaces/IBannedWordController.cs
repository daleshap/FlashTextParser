using FlashTextParser.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlashTextParser.Interfaces;
public interface IBannedWordController
{
    Task<JsonResult> GetAllBannedWordsAsync();
    Task<JsonResult> AddBannedWordAsync(BannedWord bannedWord);
    Task<JsonResult> UpdateBannedWordAsync(int id, BannedWord bannedWord);
    Task<JsonResult> DeleteBannedWordAsync(int id);

    JsonResult SanitizeText(string textToSanitize);

}
