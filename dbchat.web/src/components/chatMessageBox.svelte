<div class="chat-space {isSender ? 'sender' : 'receiver'}">
    <div class="chat-message {isSender ? 'sender' : 'receiver'}">
        <div class="inner-chat-message">{chatMessage.content}</div>
        {#if chatMessage.chart != undefined}
        <GeneratedChart chartConfig={chatMessage.chart}/>
        {/if}
        <div class="timestamp">{chatMessage.timestamp.format('HH:mm')}</div>
        {#if chatMessage.data != undefined && chatMessage.data != "" && !chatMessage.isSender}
            <button class="generate-chart" on:click={requestChart}>Generate Chart</button>
        {/if}
    </div>
</div>
<script lang="ts">
	import type { ChatMessage } from "$lib/data/ChatMessage";
    import { createEventDispatcher } from 'svelte';
	import GeneratedChart from "./generatedChart.svelte";

    export let isSender:boolean = false;
    export let chatMessage:ChatMessage;

    const dispatch = createEventDispatcher();

    function requestChart() {
        dispatch("chartRequested",chatMessage);
    }
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
        flex-wrap: wrap;
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

    .generate-chart {
        font-size: 12px;
        color: hsl(211, 100%, 50%);
        position: absolute;
        bottom: -20px;
        left:40px;
        border:none;
        background:none;
    }
</style>