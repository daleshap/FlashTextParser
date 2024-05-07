using FlashTextParser.Interfaces;
using FlashTextParser.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace FlashTextParser.Repos
{
    public class BannedWordRepository : IBannedWordRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _sqlDataSource;

        public BannedWordRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _sqlDataSource = _configuration.GetConnectionString("TextParserDatabase");
        }

        public async Task<string> CreateBannedWord(BannedWord bannedWord)
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
            return bannedWord.Word + " added to list of banned words";
        }

        public async Task<DataTable> GetAllBannedWords()
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

            return table;
        }

        public async Task<DataTable> GetBannedWord(int idKey, string word)
        {
            string query = "sp_GetBannedWord";
            DataTable table = new DataTable();
            using (SqlConnection conn = new SqlConnection(_sqlDataSource))
            {
                await conn.OpenAsync();
                using (SqlCommand command = new SqlCommand(query))
                {
                    command.Connection = conn;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@IdKey", idKey);
                    command.Parameters.AddWithValue("@Word", word);
                    using (SqlDataReader dataReader = await command.ExecuteReaderAsync())
                    {
                        table.Load(dataReader);
                    }
                    conn.Close();
                }
            }

            return table;
        }
        public async Task<string> UpdateBannedWord(BannedWord bannedWord)
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
            return "Word Updated Successfully";
        }

        public async Task<string> DeleteBannedWord([FromBody] int bannedWordId)
        {
            string query = "sp_DeleteBannedWord";
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
            return "Word Deleted Successfully";
        }

        public async Task<JsonResult> SanitizeText(string textToSanitize)
        {
            try
            {
                DataTable dt = GetAllBannedWords().Result;


                List<BannedWord> bannedWords = dt.AsEnumerable().Select(row =>
                                                                            new BannedWord
                                                                            {
                                                                                IdKey = row.Field<int>("idKey"),
                                                                                Word = row.Field<string>("word"),
                                                                                CaseSensitive = row.Field<bool>("caseSensitive"),
                                                                                WholeWordOnly = row.Field<bool>("wholeWordOnly"),
                                                                                TrimWord = row.Field<bool>("trimWord")

                                                                            }).ToList();

                string result = textToSanitize;
                foreach (BannedWord bannedWord in bannedWords.OrderByDescending(w => w.Word.Length))
                {
                    string replacementString = bannedWord.Word;
                    if (bannedWord.TrimWord)
                    {
                        replacementString = bannedWord.Word.Trim();
                    }
                    if (bannedWord.WholeWordOnly)
                    {
                        replacementString = @"\b" + replacementString + @"\b";
                    }
                    //Handle Special Characters here (this case is only *)
                    if (replacementString.Contains("*"))
                    {
                        replacementString = replacementString.Replace("*", "\\*");
                    }
                    if (bannedWord.CaseSensitive)
                    {
                        result = Regex.Replace(result, replacementString, new string('*', bannedWord.Word.Length));
                    }
                    else
                    {
                        result = Regex.Replace(result, replacementString, new string('*', bannedWord.Word.Length), RegexOptions.IgnoreCase);
                    }

                }

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                //TODO: log exception ex 
                return new JsonResult("Could not sanitize string");

            }

        }
    }
}
