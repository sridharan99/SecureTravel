﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SecureTravel
{
    /// <summary>
    /// Interaction logic for AfterLogin.xaml
    /// </summary>
    public partial class AfterLogin : Window
    {
        private OpenMessage openmessage;
        private AfterLogin currentwindow;
        private ComposeMessage composemessage;
        String email, password;
        private static IMongoClient Client = new MongoClient("mongodb://jjsridharan:test123@ds016068.mlab.com:16068/securetravel");        
        private IMongoDatabase Database = Client.GetDatabase("securetravel");
        private IMongoCollection<BsonDocument> Collection;
        List<BsonDocument> results;
        Timer timer = new Timer();
        Button[] obutton = new Button[10];
        Label[] requests;
        Button[] arequests;
        Button[] rrequests;
        int requestscount=0;
        public AfterLogin(String email, String password)
        {
            InitializeComponent();
            for (int i = 0; i < 10; ++i)
            {
                obutton[i] = new Button();
                obutton[i].Content = "From : JJSri" + i * 10 + "@gmail.com";
                obutton[i].BorderThickness = new Thickness(0);
                obutton[i].Background = Brushes.Transparent;
                obutton[i].Margin = new Thickness(36, 61 + i * 48, 0, 0);
                obutton[i].HorizontalContentAlignment = HorizontalAlignment.Stretch;
                obutton[i].VerticalAlignment = VerticalAlignment.Top;
                obutton[i].Click += action_clicked;
                this.email = email;
                this.password = password;
                this.grid.Children.Add(obutton[i]);
            }
            GetRequests();
        }
        private void DeleteRequests()
        {
            for(int i=0;i<requestscount;i++)
            {
                this.grid.Children.Remove(requests[i]);
                this.grid.Children.Remove(arequests[i]);
                this.grid.Children.Remove(rrequests[i]);
            }
        }
        private void GetRequests()
        {
            Collection = Database.GetCollection<BsonDocument>(email+"_requests");
            results = Collection.Find(new BsonDocument()).ToList();
            requests = new Label[results.Count];
            arequests = new Button[results.Count];
            rrequests = new Button[results.Count];
            requestscount = results.Count;
            int i = 0;
            foreach (var request in results)
            {
                requests[i] = new Label();
                requests[i].Content=request["mailid"].ToString();
                requests[i].Margin = new Thickness(58, 67 + i * 58, 0, 0);
                requests[i].HorizontalAlignment = HorizontalAlignment.Left;
                requests[i].VerticalAlignment = VerticalAlignment.Top;
                requests[i].Width = 372;
                requests[i].Visibility = Visibility.Hidden;
                this.grid.Children.Add(requests[i]);
                arequests[i] = new Button();
                arequests[i].Tag = i;
                arequests[i].Content = "Accept";
                arequests[i].Margin = new Thickness(448, 67 + i * 58, 0, 0);
                arequests[i].HorizontalAlignment = HorizontalAlignment.Left;
                arequests[i].VerticalAlignment = VerticalAlignment.Top;
                arequests[i].Width = 75;
                arequests[i].Visibility = Visibility.Hidden;
                arequests[i].Click += AcceptRequest;
                this.grid.Children.Add(arequests[i]);
                rrequests[i] = new Button();
                rrequests[i].Content = "Delete";
                rrequests[i].Margin = new Thickness(557, 67 + i * 58, 0, 0);
                rrequests[i].HorizontalAlignment = HorizontalAlignment.Left;
                rrequests[i].VerticalAlignment = VerticalAlignment.Top;
                rrequests[i].Width = 75;
                rrequests[i].Visibility = Visibility.Hidden;
                rrequests[i].Click += DeleteRequest;
                this.grid.Children.Add(rrequests[i]);
                Console.Write("Hello");
            }
        }
        /// <summary>
        /// When Open are Delete button is clicked for each mail.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void action_clicked(object sender, RoutedEventArgs e)
        {
            currentwindow = this;
            currentwindow.Hide();
            openmessage = new OpenMessage(email,password,currentwindow);
            openmessage.Show();
        }
        private void HandleAcceptRequest(String mailid)
        {
            Collection = Database.GetCollection<BsonDocument>(mailid+"_friends");
            var document = new BsonDocument{{ "mailid" , email }};
            Collection.InsertOne(document);
            Collection = Database.GetCollection<BsonDocument>(email + "_friends");
            document = new BsonDocument { { "mailid", mailid } };
            Collection.InsertOne(document);
            Collection = Database.GetCollection<BsonDocument>(email + "_requests");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("mailid", mailid);
            Collection.DeleteOne(filter);
            DisplayWarning("Succssfully Accepted!");
            this.Dispatcher.Invoke(new Action(() => DeleteRequests()));
            this.Dispatcher.Invoke(new Action(()=>GetRequests()));
            this.Dispatcher.Invoke(new Action(() => View_Requests(new object(),new RoutedEventArgs())));
        }
        private void AcceptRequest(object sender, RoutedEventArgs e)
        {
            String mailid = (string)requests[(int)((Button)sender).Tag].Content;
            warning.Content = "Processsing your request";
            warning.Visibility = Visibility.Visible;
            Task.Run(() => HandleAcceptRequest(mailid));
        }
        private void DeleteRequest(object sender, RoutedEventArgs e)
        {
            //Delete Friend Request Logic
        }
        private void DisplayWarning(String message, int Interval = 3000)
        {
            timer.Interval = Interval;
            warning.Dispatcher.Invoke(new Action(() => warning.Content = message));
            warning.Dispatcher.Invoke(new Action(() => warning.Visibility = Visibility.Visible));
            timer.Elapsed += (s, en) => {
                warning.Dispatcher.Invoke(new Action(() => warning.Visibility = Visibility.Hidden));
                timer.Stop();
            };
            timer.Start();
        }
        private void Compose_Message(object sender, RoutedEventArgs e)
        {
            currentwindow = this;
            currentwindow.Hide();
            composemessage = new ComposeMessage(email,password,currentwindow);
            composemessage.Show();
        }

        private void View_Requests(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                obutton[i].Visibility = Visibility.Hidden;
            }
            new_request.Visibility = Visibility.Hidden;
            new_request_mailid.Visibility = Visibility.Hidden;
            for(int i=0;i<requestscount; i++)
            {
                requests[i].Visibility = Visibility.Visible;
                arequests[i].Visibility = Visibility.Visible;
                rrequests[i].Visibility = Visibility.Visible;
            }
        }
        private void New_request_got_focus(object sender, RoutedEventArgs e)
        {
            if(((TextBox)sender).Text.Equals("Friend's Mail Id") ==true)
            {
                ((TextBox)sender).Text = "";
            }
        }
        private void New_request_lost_focus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text.Equals("") == true)
            {
                ((TextBox)sender).Text = "Friend's Mail Id";
            }
        }
        private void New_Request(object sender, RoutedEventArgs e)
        {
            for(int i=0;i<10;i++)
            {              
                obutton[i].Visibility = Visibility.Hidden;
            }
            for (int i = 0; i < requestscount; i++)
            {
                requests[i].Visibility = Visibility.Hidden;
                arequests[i].Visibility = Visibility.Hidden;
                rrequests[i].Visibility = Visibility.Hidden;
            }
            new_request_mailid.Visibility = Visibility.Visible;
            new_request.Visibility = Visibility.Visible;
        }

        public static bool IsValidEmail(string email)
        {
            Regex rx = new Regex(
            @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
            return rx.IsMatch(email);
        }
        private void Handle()
        {
            String mailid = "";
            new_request_mailid.Dispatcher.Invoke(new Action(() => mailid = new_request_mailid.Text));
            if (mailid.Equals(email) || IsValidEmail(mailid) == false)
            {
                DisplayWarning("Cannot send request to entered mail id");
                return;
            }            
            Collection = Database.GetCollection<BsonDocument>("user");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("mailid", mailid);
            results = Collection.Find(filter).ToList();
            if (results.Count == 1)
            {
                
                Collection = Database.GetCollection<BsonDocument>(email + "_requests");
                filter = builder.Eq("mailid", mailid);
                results = Collection.Find(filter).ToList();
                if (results.Count == 1)
                {
                    DisplayWarning("Request already sent. View requests to accept!");
                    return;
                }
                Collection = Database.GetCollection<BsonDocument>(mailid + "_requests");
                filter = builder.Eq("mailid", email);
                results = Collection.Find(filter).ToList();
                if (results.Count == 1)
                {
                    DisplayWarning("Friend request already sent!");
                    return;
                }
                var document = new BsonDocument{                 
                    { "mailid" , email }                    
                };
                Collection.InsertOne(document);
                DisplayWarning("Friend request sent successfully!");
            }
            else
            {
                DisplayWarning("Mail id doesn't exists!");
            }
        }

        private void Send_Request(object sender, RoutedEventArgs e)
        {
            warning.Content = "Processing your request";
            warning.Visibility = Visibility.Visible;
            Task.Run(() => Handle());
        }

        private void Inbox(object sender, RoutedEventArgs e)
        {           
            warning.Visibility = Visibility.Hidden;
            new_request_mailid.Visibility = Visibility.Hidden;
            new_request.Visibility = Visibility.Hidden;
            for (int i = 0; i < 10; i++)
            {
                obutton[i].Visibility = Visibility.Visible;
            }
        }
        private void Logout(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}
