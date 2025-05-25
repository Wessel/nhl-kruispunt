import { PriorityQueue } from '../src/priorityQueue';
import { Stopwatch } from '../src/zeromq/stopwatch';

describe('PriorityQueue', () => {
  test('enqueue adds items correctly', () => {
    const queue = new PriorityQueue();

    queue.enqueue('lane1', 2);

    expect(queue.peek()?.group).toBe('lane1');
    expect(queue.peek()?.priority).toBe(2);
  });

  test('maintains priority order', () => {
    const queue = new PriorityQueue();

    queue.enqueue('lane1', 2, 0);
    queue.enqueue('lane2', 1, 0);  // higher priority (lower number)
    queue.enqueue('lane3', 3, 0);  // lower priority (higher number)

    // Peek should show the highest priority (lowest number)
    expect(queue.peek()?.group).toBe('lane2');

    // Dequeue should return items in priority order
    expect(queue.shift()?.group).toBe('lane2');
    expect(queue.shift()?.group).toBe('lane1');
    expect(queue.shift()?.group).toBe('lane3');
  });

  test('isEmpty works correctly', () => {
    const queue = new PriorityQueue();

    expect(queue.isEmpty()).toBe(true);

    queue.enqueue('lane1', 1);
    expect(queue.isEmpty()).toBe(false);

    queue.shift();
    expect(queue.isEmpty()).toBe(true);
  });

  test('contains checks existence', () => {
    const queue = new PriorityQueue();

    queue.enqueue('lane1', 1);

    expect(queue.contains('lane1')).toBe(true);
    expect(queue.contains('lane2')).toBe(false);
  });

  test('update_priority changes priority', () => {
    const queue = new PriorityQueue();
    queue.enqueue('lane1', 2);
    queue.enqueue('lane2', 3);

    // Increase priority
    queue.update_priority('lane2', 1);

    expect(queue.peek()?.group).toBe('lane2');
    expect(queue.peek()?.priority).toBe(1);
  });

  test('get returns the correct item', () => {
    const queue = new PriorityQueue();
    queue.enqueue('lane1', 2);
    queue.enqueue('lane2', 1);

    const item = queue.get('lane1');

    expect(item).toBeDefined();
    expect(item?.group).toBe('lane1');
    expect(item?.priority).toBe(2);

    const nonexistentItem = queue.get('lane3');
    expect(nonexistentItem).toBeUndefined();
  });

  test('active items have correct status', () => {
    const clock = new Stopwatch().stop();
    const queue = new PriorityQueue();

    queue.enqueue('lane1', 2, clock.duration);

    const item = queue.get('lane1');
    expect(item).toBeDefined();
    expect(item?.activeSince).toBeUndefined();

    queue.set_active('lane1', clock.duration);
    expect(queue.get('lane1')?.activeSince).toBe(clock.duration);

    clock.forward(2000);
    expect(queue.get('lane1')?.activeSince).toBe(clock.duration - 2000);
  });
});
