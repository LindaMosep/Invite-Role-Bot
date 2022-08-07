using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using Discord.Rest;

public class InviteLinkWithRoles
{
    public string Url { get; set; }
   
    public List<ulong> RoleIds { get; set; }
    private List<ulong> list;
    public int? uses;

    public InviteLinkWithRoles(int? uses,string url, List<ulong> RoleIds)
    {
        this.uses = uses;  
        this.Url = url;
        this.RoleIds = RoleIds;
    }

   
}
internal class Program1
{
    public static DiscordSocketClient _client;
    public static List<ulong> assignRoleIDs = new List<ulong>();
    public static string BotToken;
    public static string dbPath;
    public static List<InviteLinkWithRoles> InviteLinkWithRoles = new List<InviteLinkWithRoles>();
 

  
    public static void Main()
     
     {

        BotToken = "";
        var fp = Environment.CurrentDirectory;
        dbPath = fp + "/Database";
        string db = File.ReadAllText(fp + "/Database");

        foreach(var line in db.Replace('\r', '\n').Split('\n'))
        {
            if(!string.IsNullOrWhiteSpace(line))
            {
                if(!string.IsNullOrEmpty(line))
                {
                    Console.WriteLine(line);
                    var datas = line.Split('$');
                    List<ulong> roleList = new();
                    int? uses = 0;
                    uses = int.Parse(datas[1]);
                    datas.ToList().ForEach(m => m.Replace("$", " "));
                    if (datas[2].Contains("#"))
                    {
                        foreach(var ke in Regex.Split(datas[2],"#"))
                        {
                            roleList.Add(ulong.Parse(ke.Trim()));
                        }
                    }
                    else
                    {
                        roleList.Add(ulong.Parse(datas[2].Trim()));
                    }

                  
                    var bla = new InviteLinkWithRoles(uses, datas[0].Trim(), roleList);
                    InviteLinkWithRoles.Add(bla);
                }
            }
        }


        MainAsync().Wait();
    }
    public static async Task MainAsync()
    {




        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =
            GatewayIntents.Guilds |
            GatewayIntents.GuildMembers |
            GatewayIntents.GuildMessageReactions |
            GatewayIntents.GuildMessages |
            GatewayIntents.GuildVoiceStates | GatewayIntents.All

        });




        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, BotToken);
        await _client.StartAsync();


        _client.MessageReceived += MessageHandler;
        _client.UserJoined += UserJoinedHandler;
        _client.Ready += _client_Ready;
        await Task.Delay(-1);




    }

   

 

    private static Task UserJoinedHandler(SocketGuildUser arg)
    {
        UserJoined(arg).Wait();
        return Task.CompletedTask;
    }

    public static async Task UserJoined(SocketGuildUser u)
    {
        int m = 0;
       var invites =  await u.Guild.GetInvitesAsync();
      List<InviteLinkWithRoles> remove = new List<InviteLinkWithRoles>();
        foreach (var invite in invites)
        {
            if(InviteLinkWithRoles.Find(c => c.Url.Contains(invite.Url)) != null)
            {
                m++;
                var test = InviteLinkWithRoles.Find(c => c.Url.Contains(invite.Url));
                int? countofuses = test.uses;
               
                remove.Add(test);
                if (test.uses != invite.Uses)
                {
                
                    
                    InviteLinkWithRoles.Find(c => c.Url.Contains(invite.Url)).uses = invite.Uses;

                   
                        var lines2 = File.ReadAllLines(dbPath).ToList();
                        try
                        {
                      
                            File.WriteAllText(dbPath, File.ReadAllText(dbPath).Replace(invite.Url + " $ " + countofuses, invite.Url + " $ " + invite.Uses));
                           

                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                      
                      
                    
                  
                    u.AddRolesAsync(test.RoleIds);
                }
               
                    
                
            }
        }

        foreach(var line in InviteLinkWithRoles)
        {
            var lines = File.ReadAllLines(dbPath);
            if(!remove.Contains(line))
            {
               var lg = lines.ToList().Find(m => m.Contains(line.Url));
                File.WriteAllText(dbPath, File.ReadAllText(dbPath).Replace(lg, ""));

            }
        }
       InviteLinkWithRoles.RemoveAll(m => !remove.Contains(m));

    }

    private static Task _client_Ready()
    {

        Console.WriteLine("Logined as " + _client.CurrentUser.Username + "#" + _client.CurrentUser.Discriminator);
        Console.WriteLine("Made by LindaMosep");
     
        return Task.CompletedTask;
    }

    private static Task MessageHandler(SocketMessage e)
    {
        MessageRecieved(e);
        return Task.CompletedTask;
    }

    public static async Task MessageRecieved(SocketMessage e)
    {
        var channel = e.Channel as SocketTextChannel;
        if (channel != null)
        {
            if((e.Author as IGuildUser).RoleIds.ToList().Contains(929145625281462344))
            {
                if (e.Content.StartsWith("!attach "))
                {




                    if (e.MentionedRoles.Count > 0)
                    {
                        List<string> linksinmessage = new();
                        foreach (Match item in Regex.Matches(e.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                        {
                            linksinmessage.Add(item.Value);
                        }

                        if (linksinmessage.Count > 0)
                        {
                            if (linksinmessage[0].StartsWith("https://discord.gg/"))
                            {
                              
                                string url = "";
                                if (linksinmessage[0].EndsWith("/"))
                                {
                                    url = linksinmessage[0].Remove(linksinmessage.LastIndexOf("/"));

                                }
                                else
                                {
                                    url = linksinmessage[0];

                                }

                                var invites = await channel.Guild.GetInvitesAsync();
                                
                               
                                if (channel.Guild.GetInvitesAsync().Result.ToList().Find(m => m.Url == url) != null)
                                {

                                    if (InviteLinkWithRoles.Find(m => m.Url == url) == null)
                                    {
                                      
                                        var add = new InviteLinkWithRoles(channel.Guild.GetInvitesAsync().Result.ToList().Find(m => m.Url == url).Uses, channel.Guild.GetInvitesAsync().Result.ToList().Find(m => m.Url == url).Url, (e as IMessage).MentionedRoleIds.ToList());
                                        InviteLinkWithRoles.Add(add);
                                      
                                        var st = File.ReadAllText(dbPath) + add.Url + " $ " + add.uses + " $ ";

                                        if (add.RoleIds.Count > 1)
                                        {
                                            for (int i = 0; i < add.RoleIds.Count; i++)
                                            {
                                                if (i != 0)
                                                {
                                                    st += "#" + add.RoleIds[i];
                                                }
                                                else
                                                {
                                                    st += add.RoleIds[i];
                                                }
                                            }
                                        }
                                        else
                                        {
                                            st += add.RoleIds[0];
                                        }

                                        File.WriteAllText(dbPath, st + "\n");
                                        e.Channel.SendMessageAsync("Created");
                                    }
                                    else
                                    {
                                        e.Channel.SendMessageAsync("Exist.");
                                    }

                                }
                            }
                        }
                    }
                }
                else if (e.Content.StartsWith("!remove "))
                {
                    List<string> linksinmessage = new();
                    foreach (Match item in Regex.Matches(e.Content, @"(http|ftp|https):\/\/([\w\-_]+(?:(?:\.[\w\-_]+)+))([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?"))
                    {
                        linksinmessage.Add(item.Value);
                    }

                    if (linksinmessage.Count > 0)
                    {
                        if (linksinmessage[0].StartsWith("https://discord.gg/"))
                        {

                            var m = InviteLinkWithRoles.RemoveAll(m => m.Url == linksinmessage[0]);

                            if (m > 0)
                            {
                                var lines = File.ReadAllLines(dbPath);
                                var tst = lines.ToList().Find(c => c.Contains(linksinmessage[0]));
                                File.WriteAllText(dbPath, File.ReadAllText(dbPath).Replace(tst, ""));
                                await channel.SendMessageAsync("Roles succesfully deleted.", false);
                            }
                            else
                            {
                                await channel.SendMessageAsync("You didn't attach any role to this link.", false);
                            }
                        }
                    }
                }
            }
           
         
        
        }
        



    }
    private static Task Log(LogMessage arg) 
    {
        Console.WriteLine(arg.Message);
        return Task.CompletedTask;
    }
}