using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PMDWebApplication.DB;
using PMDWebApplication.Models;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity;

namespace PMDWebApplication.Controllers
{
    public class ForumController : Controller
    {
        private dbVervoerEntities dbContext = new dbVervoerEntities();
        // GET: Forum
        public ActionResult ForumHome(int ? Id, int? page)
        {
            
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            MessageReplyViewModel vm = new MessageReplyViewModel();
            var count = dbContext.AspNetMessages.Count();

            decimal totalPages = count / (decimal)pageSize;
            ViewBag.TotalPages = Math.Ceiling(totalPages);
            vm.Messages = dbContext.AspNetMessages.OrderBy(x => x.DatePosted).ToPagedList(pageNumber, pageSize);
            ViewBag.MessagesInOnePage = vm.Messages;
            ViewBag.PageNumber = pageNumber;

            if (Id != null)
            {
                var replies = dbContext.AspNetReplies.Where(x => x.MessageId == Id.Value).OrderByDescending(x => x.ReplyDateTime).ToList();
                if (replies != null)
                {
                    foreach (var rep in replies)
                    {
                        MessageReplyViewModel.MessageReply reply = new MessageReplyViewModel.MessageReply();
                        reply.MessageId = rep.MessageId;
                        reply.Id = rep.Id;
                        reply.ReplyMessage = rep.ReplyMessage;
                        reply.ReplyDateTime = rep.ReplyDateTime;
                        reply.MessageDetails = dbContext.AspNetMessages.Where(x => x.Id == rep.MessageId).Select(s => s.Message).FirstOrDefault();
                        reply.ReplyFrom = rep.ReplyFrom;
                        vm.Replies.Add(reply);
                    }

                }
                else
                {
                    vm.Replies.Add(null);
                }


                ViewBag.MessageId = Id.Value;
            }

            return View(vm);
        }

        public ActionResult CreateForum()
        {
            MessageReplyViewModel vm = new MessageReplyViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PostMessage(MessageReplyViewModel vm)
        {
            var username = User.Identity.GetUserId();
            int msgid = 0;
            if (!string.IsNullOrEmpty(username))
            {
                var user = dbContext.AspNetUsers.SingleOrDefault(u => u.Id == username);
            }
            AspNetMessage messagetoPost = new AspNetMessage();
            if (vm.Message.Subject != string.Empty && vm.Message.Message != string.Empty)
            {
                messagetoPost.DatePosted = DateTime.Now;
                messagetoPost.Subject = HttpUtility.HtmlEncode(vm.Message.Subject);
                messagetoPost.Message = HttpUtility.HtmlEncode(vm.Message.Message);
                messagetoPost.FromUser = dbContext.AspNetUsers.Find(username).UserName;

                dbContext.AspNetMessages.Add(messagetoPost);
                dbContext.SaveChanges();
                msgid = messagetoPost.Id;
            }

            return RedirectToAction("ForumHome", "Forum", new { Id = msgid });
        }

        [HttpPost]
        public ActionResult ReplyMessage(MessageReplyViewModel vm, int messageId)
        {
            var username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = dbContext.AspNetUsers.SingleOrDefault(u => u.UserName == username);
            }
            if (vm.Reply.ReplyMessage != null)
            {
                AspNetReply _reply = new AspNetReply();
                _reply.ReplyDateTime = DateTime.Now;
                _reply.MessageId = messageId;
                _reply.ReplyFrom = User.Identity.Name;
                _reply.ReplyMessage = vm.Reply.ReplyMessage;
                dbContext.AspNetReplies.Add(_reply);
                dbContext.SaveChanges();
            }
            //reply to the message owner          - using email template

            return RedirectToAction("ForumHome", "Forum", new { Id = messageId });

        }

        [HttpPost]
        public ActionResult DeleteMessage(int messageId)
        {
            AspNetMessage _messageToDelete = dbContext.AspNetMessages.Find(messageId);
            dbContext.AspNetMessages.Remove(_messageToDelete);
            dbContext.SaveChanges();

            // also delete the replies related to the message
            var _repliesToDelete = dbContext.AspNetReplies.Where(i => i.MessageId == messageId).ToList();
            if (_repliesToDelete != null)
            {
                foreach (var rep in _repliesToDelete)
                {
                    dbContext.AspNetReplies.Remove(rep);
                    dbContext.SaveChanges();
                }
            }


            return RedirectToAction("ForumHome", "Forum");
        }
    }
}