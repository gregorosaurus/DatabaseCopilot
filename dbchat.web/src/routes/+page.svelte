<script lang="ts">
	import { ChatMessage } from "$lib/data/ChatMessage";
	import moment from "moment";
	import ChatWindow from "../components/chatWindow.svelte";
	import { ChatApi } from "$lib/services/chatApi";
	import { text } from "@sveltejs/kit";

    let chatMessages:ChatMessage[] = new Array<ChatMessage>();

    let newChatMessage:string = ""
    
    let waitingForResponse = false;

    const chatApi:ChatApi = new ChatApi();

    async function processNewChat(){
        if (newChatMessage == ""){
            return;
        }

        let chatMsg = new ChatMessage();
        chatMsg.content = newChatMessage;
        chatMsg.timestamp = moment();
        chatMessages.push(chatMsg);

        //refresh the chats
        chatMessages = chatMessages;

        //clear the chat windows
        newChatMessage = "";

        waitingForResponse = true;
        let result = "";
        try{
             result = await chatApi.sendChat(chatMsg.content);
        }catch{
            result = "Error";
        }
        waitingForResponse = false;

        let textAndData = extractTextAndData(result);

        let response = new ChatMessage();
        response.content = textAndData[0];
        response.data = textAndData[1];
        response.timestamp = moment();
        response.isSender = false;
        chatMessages.push(response);

        chatMessages = chatMessages;
    }

    function extractTextAndData(input: string): string[] {
        const regex = /^(.*?)```(.*?)```$/s;
        const match = input.match(regex);
        if (!match || match.length !== 3) {
            return [input, ""];
        }
        return [match[1].trim(), match[2].trim()];
    }

    function delay(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    function processKey(event:any) {
        if (event.key == "Enter") {
            processNewChat();
        }
    }
</script>

<div class="main">
    <div class="chat-window">
        <ChatWindow chatMessages={chatMessages} isWaitingForResponse={waitingForResponse} />
    </div>
    <div class="chat-entry-box">
        <textarea placeholder="Type your message..." bind:value={newChatMessage} on:keyup={processKey}></textarea>
        <button class="button" on:click={processNewChat}>Send</button>
    </div>
</div>

<style>
    .main {
        min-height: 100vh; /* Ensure the body takes up at least the full height of the viewport */ margin: 0;
        padding: 0;
        display: flex;
        flex-direction: column;
    }

    .chat-window {
        flex: 1; /* Allow the main div to grow and take up remaining space */
        overflow-y: auto; /* Enable vertical scrolling if content exceeds viewport height */
        padding: 20px;
        max-height:calc(100vh - 60px);
    }

    .chat-entry-box {
        display: flex;
        align-items: center;
        padding: 10px;
        background-color: #f9f9f9;
        border-top: 1px solid #ccc;
    }

    .chat-entry-box textarea {
        flex: 1;
        height: 40px;
        padding: 5px;
        border: 1px solid #ccc;
        border-radius: 5px;
        resize: none;
        margin-right: 10px;
    }

    .chat-entry-box button {
        padding: 8px 15px;
        background-color: #007bff;
        color: white;
        border: none;
        border-radius: 5px;
        cursor: pointer;
    }

    .chat-entry-box button:hover {
        background-color: #0056b3;
    }
</style>