import { Stopwatch } from "./stopwatch";

interface QueueItem {
  // Group equals to laneId in the spec
  group: string;
  priority: number;
  timestamp: number;
  activeSince?: number;
}

export class PriorityQueue {
  private _items: QueueItem[] = [];

  enqueue(group: string, priority: number, timestamp: number = Date.now()): void {
    let added = false;
    const newItem: QueueItem = { group, priority, timestamp };

    if (this.isEmpty() || newItem.priority >= this._items[this._items.length - 1].priority) {
      this._items.push(newItem);
      return;
    }

    for (let i = 0; i < this._items.length; i++) {
      if (
        newItem.priority < this._items[i].priority
        || (newItem.priority === this._items[i].priority
          && newItem.timestamp < this._items[i].timestamp)
      ) {
        this._items.splice(i, 0, newItem);
        added = true;
        break;
      }
    }

    if (!added) {
      this._items.push(newItem);
    }
  }

  shift(): QueueItem | undefined {
    return this.isEmpty() ? undefined : this._items.shift();
  }

  peek(): QueueItem | undefined {
    return this.isEmpty() ? undefined : this._items[0];
  }

  isEmpty(): boolean {
    return this._items.length === 0;
  }

  contains(group: string): boolean {
    return this._items.some(item => item.group === group);
  }

  get(group: string): QueueItem | undefined {
    return this._items.find(item => item.group === group);
  }

  updatePriority(group: string, newPriority: number): void {
    const index = this._items.findIndex(item => item.group === group);
    const item = this._items[index];

    if (index !== -1) {
      this._items.splice(index, 1);
    }

    this.enqueue(group, newPriority);
  }

  setActive(group: string, timestamp: number) {
    const item = this.get(group);

    if (item) {
      item.activeSince = timestamp;
    }
  }
}
