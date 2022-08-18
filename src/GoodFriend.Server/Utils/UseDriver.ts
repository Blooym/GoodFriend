import { DriverEnum } from '@mtypes/Drivers';

export default () => {
  switch (process.env.STORAGE_DRIVER?.toLowerCase()) {
    case 'MemoryStore'.toLowerCase(): return DriverEnum.MemoryStore;
    // case 'RedisStore'.toLowerCase(): return DriverEnum.RedisStore;
    default: return DriverEnum.MemoryStore;
  }
};
