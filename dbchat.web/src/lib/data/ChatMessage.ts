import moment from 'moment';

export class ChatMessage {
    content:string = "";
    data?:string
    timestamp:moment.Moment = moment();
    isSender:boolean = true;

    chart?:any
}