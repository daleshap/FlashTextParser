using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using FlashTextParser.Models;
using System.Text.RegularExpressions;
using FlashTextParser.Interfaces;

namespace FlashTextParser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannedWordController : ControllerBase
    {

        private readonly IBannedWordRepository _bannedWordRepository;
        public BannedWordController(IBannedWordRepository bannedWordRepository)
        {
            _bannedWordRepository = bannedWordRepository;  
        }
        [HttpPost]
        public async Task<JsonResult> CreateBannedWord(BannedWord bannedWord)
        {
            var result = await _bannedWordRepository.CreateBannedWord(bannedWord);
            return new JsonResult(result);
        }

        [HttpGet]
        public async Task<JsonResult> GetAllBannedWords()
        {          
            var result = await _bannedWordRepository.GetAllBannedWords();
            return new JsonResult(result);
        }

        [Route("GetBannedWord")]
        [HttpGet]
        public async Task<JsonResult> GetBannedWord(int idKey, string word)
        {
            var result = await _bannedWordRepository.GetBannedWord(idKey, word);
            return new JsonResult(result);
        }

        [HttpPut]
        public async Task<JsonResult> UpdateBannedWord(BannedWord bannedWord)
        {
            var result = await _bannedWordRepository.UpdateBannedWord(bannedWord);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<JsonResult> DeleteBannedWord(int bannedWordId)
        {

            var result = await _bannedWordRepository.DeleteBannedWord(bannedWordId);
            return new JsonResult(result);
        }

        [Route("SanitizeText")]
        [HttpPost]
        public async Task<JsonResult> SanitizeText([FromBody] string textToSanitize)
        {

            var result = await _bannedWordRepository.SanitizeText(textToSanitize);
            return new JsonResult(result);

        }
    }
}
