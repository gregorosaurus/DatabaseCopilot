import { PUBLIC_CHATAPI_ENDPOINT } from '$env/static/public';

export class ChatApi{
    baseApi: string = PUBLIC_CHATAPI_ENDPOINT;

    async sendChat(chat:string):Promise<string> {
        const url = this.baseApi + '/api/v1/chat';
        const requestOptions = {
            url:url,
            method: 'POST',
            headers: {
              'Content-Type': 'text/plain'
            },
            body: chat
          };
          

        const response = await this.localFetch(new Request(url, requestOptions));

        if (!response.ok){
            throw new Error("Invalid Rsponse, could not send chat message.");
        }

        return (await response.text());
    }

    async generateChart(data:string):Promise<string> {
        const url = this.baseApi + '/api/v1/chat/chart';
        const requestOptions = {
            url:url,
            method: 'POST',
            headers: {
              'Content-Type': 'text/plain'
            },
            body: data
          };
          

        const response = await this.localFetch(new Request(url, requestOptions));

        if (!response.ok){
            throw new Error("Invalid Rsponse, could not generate chart.");
        }

        return (await response.text());
    }

    private async localFetch(request:Request) : Promise<Response> {
    //add in the token. 
    // request.headers.set('Authorization', `Bearer ${await getNavigatorToken()}`);

        return await fetch(request);
    }
}