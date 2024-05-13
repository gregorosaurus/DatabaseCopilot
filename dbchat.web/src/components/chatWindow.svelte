<div class="chat-window">
    {#each chatMessages as chatMessage}
        <ChatMessageBox chatMessage={chatMessage} isSender={chatMessage.isSender}/>
    {/each}
    {#if isWaitingForResponse}
        <ChatMessageBox chatMessage={waitingChatMessage} isSender={false}/>
    {/if}
</div>

<script lang="ts">
	import { ChatMessage } from "$lib/data/ChatMessage";
    import ChatMessageBox from "./chatMessageBox.svelte";
    import {onMount} from 'svelte'

    export let chatMessages:ChatMessage[]

    export let isWaitingForResponse:boolean = false;

    const waitingChatMessage:ChatMessage = new ChatMessage();

    onMount(()=>{
        waitingChatMessage.content = "...";
        waitingChatMessage.isSender = false;
    });
</script>

<style>
    .chat-window {
        display:flex;
        flex-direction: column;
        row-gap: 20px;
    }
</style>