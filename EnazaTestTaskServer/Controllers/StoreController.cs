using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnazaTestTaskServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IFileService _fileService;

        public StoreController(IFileService fileService) : base()
        {
            _fileService = fileService;
        }

        [HttpGet]
        public string GetStore()
        {
            return _fileService.GetStoredMessage() ?? String.Empty;
        }
    }
}