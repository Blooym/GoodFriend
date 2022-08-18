// /* eslint-disable no-console */
// import IDataDriver from '@interfaces/IDataDriver';
// import { createClient } from 'redis';

// class RedisStore implements IDataDriver {
//   private client: any;

//   constructor() {
//     this.client = createClient({
//       url: process.env.STORAGE_DRIVER_URL,
//     });

//     this.client.connect().catch((err: Error) => {
//       console.error(err);
//       process.exit(1);
//     });
//   }

//   public async getKey(key: string) {
//     return this.client.get(key);
//   }

//   public addKey(key: string, data: any) {
//     return this.client.set(key, data);
//   }

//   public delKey(key: string) {
//     return this.client.del(key);
//   }

//   public async findValue(value: string) {
//     return this.client.keys(`*${value}*`);
//   }

//   public async length() {
//     return this.client.dbSize();
//   }

//   public async forEach(callback: Function) {
//     return this.client.keys('*', (err: Error, keys: string[]) => {
//       if (err) {
//         console.error(err);
//       }

//       keys.forEach((key) => {
//         callback(key, this.client.get(key));
//       });
//     });
//   }
// }

// export default RedisStore;
