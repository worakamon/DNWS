using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {
        private List<User> GetUser()
        {
            using(var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("following");
            string message = request.getRequestByKey("message");
            string timeline = request.getRequestByKey("timeline");
            string[] site = request.Filename.Split("?");

            try
            {
                if (site[0] == "users")
                {
                    if (request.Method == "GET")
                    {
                        //JSON is easy use to convert between JSON and .Net in these case the response code(200,400,...)
                        //Ref. https://www.newtonsoft.com/json/help/html/SerializingJSON.htm
                        string js = JsonConvert.SerializeObject(GetUser());
                        response.body = Encoding.UTF8.GetBytes(js);
                    }
                    else if (request.Method == "POST")
                    {
                        if (user != null && password != null)
                        {
                            Twitter.AddUser(user, password);
                        }
                    }
                    else if (request.Method == "DELETE")
                    {
                        if (user != null)
                        {
                            Twitter.RemoveUser(user);
                        }
                    }
                }
                else if (site[0] == "following")
                {
                    if (request.Method == "GET")
                    {
                        Twitter twit = new Twitter(user);
                        string js = JsonConvert.SerializeObject(twit.GetFollowing());
                        response.body = Encoding.UTF8.GetBytes(js);
                    }
                    else if (request.Method == "POST")
                    {
                        if (user != null && password != null)
                        {
                            Twitter twit = new Twitter(user);
                            twit.AddFollowing(following);
                        }
                    }
                    else if (request.Method == "DELETE")
                    {
                        if (user != null && password != null)
                        {
                            Twitter twit = new Twitter(user);
                            twit.RemoveFollowing(following);
                        }
                    }
                    else if (site[0] == "tweets")
                    {
                        if (user != null)
                        {
                            if (request.Method == "GET")
                            {
                                Twitter twit = new Twitter(user);
                                string js = JsonConvert.SerializeObject(twit.GetUserTimeline());
                                response.body = Encoding.UTF8.GetBytes(js);
                                if (timeline != null)
                                {
                                    string json = JsonConvert.SerializeObject(twit.GetFollowingTimeline());
                                    response.body = Encoding.UTF8.GetBytes(json);
                                }
                            }
                            else if (request.Method == "POST")
                            {
                                Twitter twit = new Twitter(user);
                                twit.PostTweet(message);

                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                Console.WriteLine(ex.ToString());
                sb.Append(String.Format("Error [{0}], please go back to <a href=\"/twitter\">login page</a> to try again", ex.Message));
                response.body = Encoding.UTF8.GetBytes(sb.ToString());
                return response;
            }
            return response;
        }
    }
}