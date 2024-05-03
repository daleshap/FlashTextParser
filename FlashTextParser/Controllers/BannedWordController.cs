using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Collections.Generic;
using System.Data.SqlClient;
using FlashTextParser.Models;
using System.Text.RegularExpressions;

namespace FlashTextParser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannedWordController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string _sqlDataSource;
        public BannedWordController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _sqlDataSource = _configuration.GetConnectionString("TextParserDatabase");
        }

        [HttpPost]
        public async Task<JsonResult> Post(BannedWord bannedWord)
        {
            string query = "sp_CreateBannedWord";
            using (SqlConnection conn = new SqlConnection(_sqlDataSource))
            {
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(query))
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Word", bannedWord.Word);
                    command.Parameters.AddWithValue("@CaseSensitive", bannedWord.CaseSensitive);
                    command.Parameters.AddWithValue("@WholeWordOnly", bannedWord.WholeWordOnly);
                    command.Parameters.AddWithValue("@TrimWord", bannedWord.TrimWord);
                    await command.ExecuteNonQueryAsync();
                    conn.Close();
                }
            }
            return new JsonResult(bannedWord.Word + " added to list of banned words");
        }


        [HttpGet]
        public async Task<JsonResult> Get()
        {
            string query = "sp_GetAllBannedWords";
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(_sqlDataSource))
            {
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(query))
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        table.Load(dataReader);
                    }
                    conn.Close();
                }
            }

            return new JsonResult(table);
        }

        [HttpPut]
        public async Task<JsonResult> Put(BannedWord bannedWord)
        {
            string query = "sp_UpdateBannedWord";
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(_sqlDataSource))
            {
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(query))
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@WordId", bannedWord.IdKey); 
                    command.Parameters.AddWithValue("@Word", bannedWord.Word);
                    command.Parameters.AddWithValue("@CaseSensitive", bannedWord.CaseSensitive);
                    command.Parameters.AddWithValue("@WholeWordOnly", bannedWord.WholeWordOnly);
                    command.Parameters.AddWithValue("@TrimWord", bannedWord.TrimWord);
                    await command.ExecuteNonQueryAsync();
                }
                conn.Close();
            }
            return new JsonResult("Word Updated Successfully");
        }

        [HttpDelete]
        public async Task<JsonResult> Delete([FromBody] int bannedWordId)
        {
            string query = "sp_DeleteBannedWord";
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(_sqlDataSource))
            {
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(query))
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@WordId", bannedWordId);
                    await command.ExecuteNonQueryAsync();
                }
                conn.Close();
            }
            return new JsonResult(table);
        }

        [Route("SanitizeText")]
        [HttpPost]
        public async Task<JsonResult> SanitizeText(string textToSanitize)
        {
            try
            {
                DataTable dt = (DataTable)Get().Result.Value;
                

                List<BannedWord> bannedWords = dt.AsEnumerable().Select(row =>
                                                                            new BannedWord
                                                                            {
                                                                                IdKey = row.Field<int>("idKey"),
                                                                                Word= row.Field<string>("word"),
                                                                                CaseSensitive = row.Field<bool>("caseSensitive"),
                                                                                WholeWordOnly = row.Field<bool>("wholeWordOnly"),
                                                                                TrimWord= row.Field<bool>("trimWord")

                                                                            }).ToList();

                string result = textToSanitize;
                foreach (BannedWord bannedWord in bannedWords)
                {
                    if(bannedWord.TrimWord)
                    {
                        bannedWord.Word = bannedWord.Word.Trim();
                    }
                    if(bannedWord.CaseSensitive)
                    {
                        if (bannedWord.WholeWordOnly)
                        {
                            result = Regex.Replace(result, " " + bannedWord.Word + " ", " " + new string('*', bannedWord.Word.Length) + " ");
                        }
                        else
                        {
                            result = Regex.Replace(result, bannedWord.Word, new string('*', bannedWord.Word.Length));
                        }
                    }
                    else
                    {
                        if (bannedWord.WholeWordOnly)
                        {
                            result = Regex.Replace(result, " " + bannedWord.Word + " ", " " + new string('*', bannedWord.Word.Length) + " ", RegexOptions.IgnoreCase);
                        }
                        else
                        {
                            result = Regex.Replace(result, bannedWord.Word, new string('*', bannedWord.Word.Length), RegexOptions.IgnoreCase);
                        }
                    }
                                        
                }

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                //TODO: log exception ex 
                return new JsonResult("anonymous.png");

            }

        }

    }
}
