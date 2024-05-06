using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FlashTextParser.Controllers;
using FlashTextParser.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace FlashTextParser.Tests.Controllers
{
    [TestFixture]
    public class BannedWordControllerTests
    {
        private BannedWordController _controller;
        private Mock<IConfiguration> _configurationMock;
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

                _controller = new BannedWordController(configuration);

                
                _testWord = Guid.NewGuid().ToString();
            }
            catch (Exception ex)
            {
                var x = ex.InnerException;
            }
        }

        [Test]
        public async Task Post_ReturnsJsonResult_WithCorrectMessage()
        {
            // Arrange
            var bannedWord = new BannedWord { Word = _testWord, CaseSensitive = true, WholeWordOnly = false, TrimWord = false };

            // Act
            var result = await _controller.CreateBannedWord(bannedWord);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            Assert.AreEqual($"{_testWord} added to list of banned words", jsonResult.Value);
        }

        [Test]
        public async Task Get_ReturnsJsonResult_WithDataTable()
        {
            // Arrange

            // Act
            var result = await _controller.GetAllBannedWords();

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            Assert.IsInstanceOf<DataTable>(jsonResult.Value);
        }
        [Test]
        public async Task TestCRUDProcess()
        {
            // Create Word
            var bannedWord = new BannedWord { Word = _testWord, CaseSensitive = true, WholeWordOnly = false, TrimWord = false };
            var result = await _controller.CreateBannedWord(bannedWord);
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            Assert.AreEqual($"{_testWord} added to list of banned words", jsonResult.Value);


            // GetWordByName
            result = await _controller.GetBannedWord(0, _testWord);
            DataTable dt = (DataTable)result.Value;
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
            Assert.IsInstanceOf<JsonResult>(result);
            jsonResult = (JsonResult)result;
            Assert.IsInstanceOf<DataTable>(jsonResult.Value);
            Assert.AreEqual(_testWord, bannedWords.FirstOrDefault().Word);



            // GetWordById
            result = await _controller.GetBannedWord(_testWordId, "");
             dt = (DataTable)result.Value;
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
            Assert.IsInstanceOf<JsonResult>(result);
            jsonResult = (JsonResult)result;
            Assert.IsInstanceOf<DataTable>(jsonResult.Value);
            Assert.AreEqual(_testWord, bannedWords.FirstOrDefault().Word);

            //Update Word
            // Arrange
            bannedWord = new BannedWord { IdKey = _testWordId, Word = "test", CaseSensitive = true, WholeWordOnly = false, TrimWord = false };

            // Act
            result = await _controller.UpdateBannedWord(bannedWord);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
            jsonResult = (JsonResult)result;
            Assert.AreEqual("Word Updated Successfully", jsonResult.Value);


            //Delete Word
            result = await _controller.DeleteBannedWord(_testWordId);

            // Assert
            Assert.IsInstanceOf<JsonResult>(result);
            jsonResult = (JsonResult)result;
            Assert.IsInstanceOf<DataTable>(jsonResult.Value);

        }



        [Test]
        public async Task SanitizeText_ReturnsJsonResult_WithSanitizedText()
        {
            // Arrange
            var result = await _controller.GetAllBannedWords();
            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = (JsonResult)result;
            DataTable dt = (DataTable)result.Value;
            List<BannedWord> bannedWords = dt.AsEnumerable().Select(row =>
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
                result = _controller.SanitizeText(bannedWord.Word.Trim());

                // Assert
                Assert.IsInstanceOf<JsonResult>(result);
                jsonResult = (JsonResult)result;
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