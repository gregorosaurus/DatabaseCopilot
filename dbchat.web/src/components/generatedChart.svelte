<canvas id={chartId}>

</canvas>

<script lang="ts">
    import Chart from 'chart.js/auto'
    import {createEventDispatcher, onMount} from 'svelte'
    
    export let chartConfig:any;

    const chartId = "chart-"+(Math.random() * 10e15).toString(16);

    onMount(() => {
        const ctx = (document.getElementById(chartId) as HTMLCanvasElement).getContext('2d')!;
        new Chart(ctx, chartConfig);
    });
</script>