<div class="chat-space {isSender ? 'sender' : 'receiver'}">
    <div class="chat-message {isSender ? 'sender' : 'receiver'}">
        <div class="inner-chat-message">{chatMessage.content}</div>
        <div class="timestamp">{chatMessage.timestamp.format('HH:mm')}</div>
    </div>
</div>
<script lang="ts">
	import type { ChatMessage } from "$lib/data/ChatMessage";

    export let isSender:boolean = false;
    export let chatMessage:ChatMessage
</script>

<style>
    .chat-space {
        display: flex;
        width:100%;
    }

    .chat-space.sender {
        justify-content: end;
    }
    .chat-space.receiver {
        justify-content: start;
    }

    .chat-message {
        max-width: 70%;
        min-width:50px;
        padding: 8px;
        border-radius: 15px;
        position: relative;
        margin-bottom:6px;
        display:flex;
        white-space: pre-wrap;
    }
    .chat-message.sender {
        background-color: #007bff;
        color: white;
        justify-content: end;
    }
    .chat-message.receiver {
        background-color: #f1f0f0;
        color: #333;
        justify-content: start;
    }
    .timestamp {
        font-size: 12px;
        color: #666;
        position: absolute;
        bottom: -20px;
    }

    .chat-message.sender .timestamp {
        right:0px;
    }
</style>