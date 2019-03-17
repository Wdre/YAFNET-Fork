﻿<%@ Control Language="c#" AutoEventWireup="True" Inherits="YAF.Pages.im_xmpp" Codebehind="im_xmpp.ascx.cs" %>

<YAF:PageLinks runat="server" ID="PageLinks" />

<div class="row">
    <div class="col-xl-12">
        <h2><YAF:LocalizedLabel ID="LocalizedLabel6" runat="server" LocalizedTag="TITLE" /></h2>
    </div>
</div>

<div class="row">
    <div class="col">
        <YAF:Alert runat="server" ID="Alert">
            <div class="text-center">
                <asp:Label ID="NotifyLabel" runat="server"></asp:Label>
            </div>
        </YAF:Alert>
    </div>
</div>

<div id="DivSmartScroller">
    <YAF:SmartScroller ID="SmartScroller1" runat="server" />
</div>
