﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;
using WebApi.Dtos;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using WebApi.Helpers;
using System.Linq;
namespace WebApi.Controllers
{

    [Authorize]
    [Route("[controller]")]
    public class MessageController : Controller
    {
        private IMessageService _msgService;
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public MessageController(
            IMessageService msgService,
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _msgService = msgService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _userService = userService;
        }
        // [EnableCors("CorsPolicy")]
        [HttpPost("save")]
        public IActionResult save([FromBody]MessageDto msgDto)
        {

            try
            {
                var userTo = _userService.GetByEmail(msgDto.To);
                var userId = int.Parse(HttpContext.User.Identity.Name);
                var userFrom = _userService.GetById(userId);
                var msg = new Message()
                {
                    ActionDateTime = DateTime.Now,
                    IsReceiverDeleted = false,
                    IsSenderDeleted = false,
                    IsSent = msgDto.ShouldSent,
                    MessageContent = msgDto.Content,
                    Title = msgDto.Title,
                    UserReceiverId = userTo.Id,
                    UserSenderId = userId
                };

                _msgService.Save(msg);

                return Ok();
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _msgService.Delete(id);
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var msg = _msgService.GetById(id);
            var from  = _userService.GetById(msg.UserSenderId);
            var to = _userService.GetById(msg.UserReceiverId.Value);
            return Ok(
                    new
                    {
                        Title = msg.Title,
                        MessageContent = msg.MessageContent,
                        Id = msg.Id,
                        From = from.FirstName + " " + from.LastName,
                        To = to.FirstName + " " + to.LastName,
                        Date = msg.ActionDateTime.ToPersianDate(),
                    });

        }

        [Route("inbox")]
        [HttpGet]
        public IActionResult GetInbox()
        {
            var userId = int.Parse(HttpContext.User.Identity.Name);
            var msg = _msgService.GetInbox(userId).Select(x => new DtoMessaeItem
            {
                Date = x.ActionDateTime.ToPersianDate(),
                Id = x.Id.ToString(),
                From = _userService.GetById(x.UserReceiverId.Value).FirstName + " " +
                _userService.GetById(x.UserReceiverId.Value).LastName,
                Title = x.Title
            });

            return Ok(msg);
        }

        [Route("draft")]
        [HttpGet]
        public IActionResult GetDrafts()
        {
            var userId = int.Parse(HttpContext.User.Identity.Name);
            var msg = _msgService.GetDrafts(userId).Select(x => new DtoMessaeItem
            {
                Date = x.ActionDateTime.ToPersianDate(),
                Id = x.Id.ToString(),
                To = _userService.GetById(x.UserReceiverId.Value).FirstName + " " +
                _userService.GetById(x.UserReceiverId.Value).LastName,
                Title = x.Title
            });


            return Ok(msg);
        }

        [Route("sents")]
        [HttpGet]
        public IActionResult Sents()
        {
            var userId = int.Parse(HttpContext.User.Identity.Name);
            var msg = _msgService.GetSents(userId).Select(x => new DtoMessaeItem
            {
                Date = x.ActionDateTime.ToPersianDate(),
                Id = x.Id.ToString(),
                To = _userService.GetById(x.UserReceiverId.Value).FirstName + " " +
                _userService.GetById(x.UserReceiverId.Value).LastName,
                Title = x.Title
            });


            return Ok(msg);
        }
    }
}
