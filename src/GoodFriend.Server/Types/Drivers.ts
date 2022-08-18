/* eslint-disable no-unused-vars */
/* eslint-disable no-shadow */
import MemoryStore from '@data/Drivers/MemoryStore';
// import RedisStore from '@data/Drivers/RedisStore';

export enum DriverEnum {
    MemoryStore = 'MemoryStore',
    // RedisStore = 'RedisStore',
}

// export type DriverTypes = MemoryStore | RedisStore;
export type DriverTypes = MemoryStore;
