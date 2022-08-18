import { DriverTypes, DriverEnum } from '@mtypes/Drivers';
import MemoryStore from '@data/Drivers/MemoryStore';
// import RedisStore from '@data/Drivers/RedisStore';
import { Response } from 'express';

// wrapper class around the cache driver
export default class ClientStore {
  private cache: DriverTypes;

  constructor(cache: DriverEnum) {
    switch (cache) {
      case DriverEnum.MemoryStore:
        this.cache = new MemoryStore();
        break;
      // case DriverEnum.RedisStore:
      //   this.cache = new RedisStore();
      //   break;
      default:
        this.cache = new MemoryStore();
        break;
    }
  }

  public getKey(key: string) {
    return this.cache.getKey(key);
  }

  public addKey(key: string, data: Response) {
    return this.cache.addKey(key, data);
  }

  public delKey(key: string) {
    return this.cache.delKey(key);
  }

  public findValue(value: any) {
    return this.cache.findValue(value);
  }

  public length() {
    return this.cache.length();
  }

  public forEach(callback: Function) {
    return this.cache.forEach(callback);
  }
}
