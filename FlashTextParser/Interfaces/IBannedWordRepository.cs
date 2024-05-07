using FlashTextParser.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace FlashTextParser.Interfaces
{
    public interface IBannedWordRepository
    {
        Task<string> CreateBannedWord(BannedWord bannedWord);

        Task<DataTable> GetAllBannedWords();

        Task<DataTable> GetBannedWord(int idKey, string word);

        Task<string> UpdateBannedWord(BannedWord bannedWord);

        Task<string> DeleteBannedWord([FromBody] int bannedWordId);

        Task<JsonResult> SanitizeText([FromBody] string textToSanitize);
       
    }
}
