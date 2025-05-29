interface ParsedArgs {
  _: (string | number | boolean)[];
  [key: string]: string | number | boolean | (string | number | boolean)[];
}

/**
 * Parse arguments of an array
 * @param args - The arguments to parse
 * @returns The parsed arguments
 */
export default (args: string[]): ParsedArgs => {
  const p: { [key: string]: string | number | boolean } = {};
  const l: (string | number | boolean)[] = [];

  const rHyphens = (v: string): string => v.replace(/^\-+/g, '');
  const cApplicable = (v: string): string | number | boolean => {
    if (isNaN(Number(v))) {
      const lower = v.toString().toLowerCase();
      return lower === 'true' ? true : (lower === 'false' ? false : v);
    }
    return Number(v);
  };

  for (let _ = 0; _ < args.length; _++) {
    const e = args[_].indexOf('=');
    const r = args[_].charAt(0) === '-' && args.length - 1 >= _ + 1 && args[_ + 1].indexOf('=') === -1 && args[_ + 1].charAt(0) !== '-';
    const n = e === -1 ? rHyphens(args[_]) : rHyphens(args[_].slice(0, e));

    if (e !== -1) p[n] = cApplicable(args[_].slice(e + 1));
    else if (r) {
      p[n] = cApplicable(args[_ + 1]);
      _++;
    } else if (args[_].charAt(0) === '-') {
      if (args[_].charAt(1) === '-') p[n] = true;
      else for (let b = 0; b < n.length; b++) p[n.charAt(b)] = true;
    } else l.push(cApplicable(n));
  }

  return Object.assign(p, { '_': l });
};
