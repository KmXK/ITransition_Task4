"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

connection.on("Receive", function (lastMessage, broCount, sisCount) {
    updateStats(lastMessage, broCount, sisCount);
});

connection.start();

function updateStats(lastMessage, broCount, sisCount) {
    if(document.getElementById("lastMessage") != undefined)
        document.getElementById("lastMessage").innerHTML = lastMessage;
    document.getElementById("broCount").innerHTML = broCount;
    document.getElementById("sisCount").innerHTML = sisCount;
}