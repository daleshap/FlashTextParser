using Microsoft.AspNetCore.Mvc;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FlashTextParser.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;
using FlashTextParser.Repos;

namespace FlashTextParser.Tests.Controllers
{
    [TestFixture]
    public class BannedWordRepositoryTests
    {
        private BannedWordRepository _bannedWordRepository;
        private string _testWord;
        private int _testWordId = 0;

        [SetUp]
        public void Setup()
        {
            try
            {
                var configValues = new Dictionary<string, string>
                {
                    { "ConnectionStrings:TextParserDatabase", "Data Source=.;Initial Catalog=TextParserApi;Integrated security=true;" }
                };
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(configValues)
                    .Build();

                _bannedWordRepository = new BannedWordRepository(configuration);


                _testWord = Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                var x = ex.InnerException;
            }
        }

        [Test]
        public async Task Post_WithCorrectMessage()
        {
            // Arrange
            var bannedWord = new BannedWord { Word = _testWord, CaseSensitive = true, WholeWordOnly = false, TrimWord = false };

            // Act
            var result = await _bannedWordRepository.CreateBannedWord(bannedWord);

            // Assert
            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual($"{_testWord} added to list of banned words", result);
        }

        [Test]
        public async Task Get_Returns_WithDataTable()
        {
            // Arrange

            // Act
            var result = await _bannedWordRepository.GetAllBannedWords();

            // Assert
            Assert.IsInstanceOf<DataTable>(result);
        }
        [Test]
        public async Task TestCRUDProcess()
        {
            // Create Word
            var bannedWord = new BannedWord { Word = _testWord, CaseSensitive = true, WholeWordOnly = false, TrimWord = false };
            var addResult = await _bannedWordRepository.CreateBannedWord(bannedWord);
            Assert.AreEqual($"{_testWord} added to list of banned words", addResult);


            // GetWordByName
            var getResult = await _bannedWordRepository.GetBannedWord(0, _testWord);
            DataTable dt = getResult;
            List<BannedWord> bannedWords = dt.AsEnumerable().Select(row =>
                                                                        new BannedWord
                                                                        {
                                                                            IdKey = row.Field<int>("idKey"),
                                                                            Word = row.Field<string>("word"),
                                                                            CaseSensitive = row.Field<bool>("caseSensitive"),
                                                                            WholeWordOnly = row.Field<bool>("wholeWordOnly"),
                                                                            TrimWord = row.Field<bool>("trimWord")

                                                                        }).ToList();

            Assert.AreEqual(1, bannedWords.Count);
            _testWordId = bannedWords.FirstOrDefault().IdKey;
            // Assert
            Assert.IsInstanceOf<DataTable>(getResult);
            Assert.AreEqual(_testWord, bannedWords.FirstOrDefault().Word);



            // GetWordById
            var getSingleResult = await _bannedWordRepository.GetBannedWord(_testWordId, "");
            dt = getSingleResult;
            bannedWords = dt.AsEnumerable().Select(row =>
                                                                        new BannedWord
                                                                        {
                                                                            IdKey = row.Field<int>("idKey"),
                                                                            Word = row.Field<string>("word"),
                                                                            CaseSensitive = row.Field<bool>("caseSensitive"),
                                                                            WholeWordOnly = row.Field<bool>("wholeWordOnly"),
                                                                            TrimWord = row.Field<bool>("trimWord")

                                                                        }).ToList();

            Assert.AreEqual(1, bannedWords.Count);
            _testWordId = bannedWords.FirstOrDefault().IdKey;
            // Assert
            Assert.IsInstanceOf<DataTable>(getSingleResult);
            Assert.AreEqual(_testWord, bannedWords.FirstOrDefault().Word);

            //Update Word
            // Arrange
            bannedWord = new BannedWord { IdKey = _testWordId, Word = "test", CaseSensitive = true, WholeWordOnly = false, TrimWord = false };

            // Act
            var updateResult = await _bannedWordRepository.UpdateBannedWord(bannedWord);

            // Assert
            Assert.IsInstanceOf<string>(updateResult);
            Assert.AreEqual("Word Updated Successfully", updateResult);


            //Delete Word
            var deleteResult = await _bannedWordRepository.DeleteBannedWord(_testWordId);

            // Assert
            Assert.IsInstanceOf<string>(deleteResult);

        }


        
        [Test]
        public async Task SanitizeText_Returns_WithSanitizedText()
        {
            // Arrange
            var bannedWordsdt = await _bannedWordRepository.GetAllBannedWords();
            Assert.IsInstanceOf<DataTable>(bannedWordsdt);
            List<BannedWord> bannedWords = bannedWordsdt.AsEnumerable().Select(row =>
                                                                        new BannedWord
                                                                        {
                                                                            IdKey = row.Field<int>("idKey"),
                                                                            Word = row.Field<string>("word"),
                                                                            CaseSensitive = row.Field<bool>("caseSensitive"),
                                                                            WholeWordOnly = row.Field<bool>("wholeWordOnly"),
                                                                            TrimWord = row.Field<bool>("trimWord")

                                                                        }).ToList();
            
            foreach(var bannedWord in bannedWords)
            {
                var result = _bannedWordRepository.SanitizeText(bannedWord.Word.Trim());

                // Assert
                Assert.IsInstanceOf<JsonResult>(result.Result);
                var jsonResult = result.Result;
                Assert.IsInstanceOf<string>(jsonResult.Value);
                var sanitizedText = (string)jsonResult.Value;
                Assert.AreEqual(new string('*', bannedWord.Word.Length), sanitizedText);
            }
            
        }



        /*
        [Test]
        foreach(


        */
    }
}