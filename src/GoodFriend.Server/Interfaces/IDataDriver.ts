/* eslint-disable no-unused-vars */

interface IDataDriver {
    getKey(key: string): Promise<any>;
    addKey(key: string, value: any): any;
    delKey(key: string): any;
    findValue(value: any): Promise<any>;
    length(): Promise<number>
    forEach(callback: Function): Promise<any>;
}

export default IDataDriver;
