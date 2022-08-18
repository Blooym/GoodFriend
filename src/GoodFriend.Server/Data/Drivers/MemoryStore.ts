import IDataDriver from '@interfaces/IDataDriver';

class MemoryStore implements IDataDriver {
  private data: any = {};

  public async getKey(key: string) {
    return this.data[key];
  }

  public addKey(key: string, data: any) {
    this.data[key] = data;
  }

  public delKey(key: string) {
    delete this.data[key];
  }

  public async findValue(value: any) {
    return Object.keys(this.data).filter((key) => this.data[key] === value);
  }

  public async length() {
    return Object.keys(this.data).length;
  }

  public async forEach(callback: Function) {
    Object.keys(this.data).forEach((key) => callback(key, this.data[key]));
  }
}

export default MemoryStore;
