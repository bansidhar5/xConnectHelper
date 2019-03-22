﻿using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.SharedSource.XConnectHelper.Helper;
using Sitecore.SharedSource.XConnectHelper.Model;
using System;
using System.Collections.Generic;
using System.Web;
using Sitecore.SharedSource.XConnectHelper.Impl;
using System.Linq;

namespace Sitecore.SharedSource.XConnectHelper.sitecore_modules.Web.xConnect
{
    public partial class XConnectHelper : System.Web.UI.Page
    {
        private IXConnectService _helper;
        protected ContactData Contact;
        protected List<string> Messages = new List<string>();
        protected IList<XConnectValidator> Status;
        protected SessionData SessionData;

        protected override void OnInit(EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.GetSetting("xConnectHelper.AccessKey")))
            {
                var message = "xConnectHelper: No 'xConnectHelper.AccessKey' setting has been set in config.";
                Log.Error(message, this);
                throw new UnauthorizedAccessException(message);
            }

            if (Settings.GetSetting("xConnectHelper.AccessKey") != HttpContext.Current.Request.QueryString.Get("key"))
            {
                var message = "xConnectHelper: The provided key doesn't match the 'xConnectHelper.AccessKey' setting.";
                Log.Error(message, this);
                throw new UnauthorizedAccessException(message);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitPage();
            }
        }

        protected void InitPage()
        {
            _helper = new XConnectService();
            _helper.DontTrackPageView();

            Contact = new ContactData();
            SessionData = _helper.SessionData;

            var collectionValidator = new CollectionValidator("Collection", "xconnect.collection");

            Status = new List<XConnectValidator>
            {
               collectionValidator,
               new XConnectValidator("MA Operations", "xdb.marketingautomation.operations.client"),
               new XConnectValidator("MA Reporting", "xdb.marketingautomation.reporting.client"),
               new XConnectValidator("Referencedata", "xdb.referencedata.client")
            };

            foreach (var val in Status)
            {
                val.Validate();
            }

            if (collectionValidator.Error)
            {
                Messages.AddRange(collectionValidator.Messages);
            }         

            if (!_helper.IsTrackerActive)
            {
                Messages.Add("Tracker is not active! Check configuration and license.");                
            }

            // Only read contact from xConnect when it is available
            if (!collectionValidator.Error && _helper.IsTrackerActive)            
            {
                Contact = _helper.Contact;
                Firstname.Text = Contact.Firstname;
                Lastname.Text = Contact.Lastname;
                EmailAddress.Text = Contact.PreferredEmail;
            }

            Messages.AddRange(_helper.ValidateConfig());
        }

        protected void FlushSession_Click(object sender, EventArgs e)
        {
            _helper.FlushSession();
            Messages.Add("Session has been flushed");
            InitPage();
        }

        protected void IdentifyContact_Click(object sender, EventArgs e)
        {
            _helper.SetIdentifier(Identifier.Text, IdentifierSource.Text);
            InitPage();
        }

        protected void SetContactData_Click(object sender, EventArgs e)
        {
            _helper.SetContactData(Firstname.Text, Lastname.Text, EmailAddress.Text);
            InitPage();
        }
    }
}