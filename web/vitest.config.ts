import {defineConfig, mergeConfig} from 'vitest/config';
import viteConfig from './vite.config';

// Riusa gli alias di vite.config.ts. I test coprono mapper, utility e repository:
// non serve un DOM, quindi girano in ambiente Node.
export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'node',
      include: ['src/**/*.test.ts']
    }
  })
);
