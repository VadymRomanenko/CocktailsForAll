import { useCallback, useEffect, useRef, useState } from 'react';
import { X } from 'lucide-react';
import { searchIngredients } from '../../api/ingredients';
import type { Ingredient } from '../../api/ingredients';

export type SelectedIngredient = { id: number; name: string } | { id: null; name: string };

interface IngredientSearchBarProps {
  value: SelectedIngredient[];
  onChange: (value: SelectedIngredient[]) => void;
  placeholder?: string;
}

export function IngredientSearchBar({ value, onChange, placeholder = 'Search ingredients...' }: IngredientSearchBarProps) {
  const [input, setInput] = useState('');
  const [suggestions, setSuggestions] = useState<Ingredient[]>([]);
  const [open, setOpen] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const fetchSuggestions = useCallback(async (term: string) => {
    if (!term.trim()) {
      setSuggestions([]);
      return;
    }
    const items = await searchIngredients(term, 15);
    const selectedIds = new Set(value.filter((v): v is { id: number; name: string } => v.id !== null).map((v) => v.id));
    setSuggestions(items.filter((i) => !selectedIds.has(i.id)));
  }, [value]);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => fetchSuggestions(input), 200);
    return () => { if (debounceRef.current) clearTimeout(debounceRef.current); debounceRef.current = null; };
  }, [input, fetchSuggestions]);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) setOpen(false);
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const addIngredient = (item: SelectedIngredient) => {
    if (value.some((v) => (v.id !== null && item.id !== null && v.id === item.id) || v.name === item.name)) return;
    onChange([...value, item]);
    setInput('');
    setSuggestions([]);
    setOpen(false);
  };

  const removeIngredient = (idx: number) => {
    onChange(value.filter((_, i) => i !== idx));
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && input.trim()) {
      e.preventDefault();
      addIngredient({ id: null, name: input.trim() });
    }
  };

  return (
    <div ref={containerRef} className="relative">
      <div className="flex flex-wrap gap-2 p-2 bg-amber-900/50 rounded-xl border border-amber-700/50 min-h-[44px]">
        {value.map((v, i) => (
          <span
            key={v.id ?? v.name + i}
            className="inline-flex items-center gap-1 px-2 py-1 rounded-lg bg-amber-800/80 text-amber-100 text-sm"
          >
            {v.name}
            <button
              type="button"
              onClick={() => removeIngredient(i)}
              className="hover:text-amber-300 p-0.5"
              aria-label="Remove"
            >
              <X size={14} />
            </button>
          </span>
        ))}
        <input
          type="text"
          value={input}
          onChange={(e) => { setInput(e.target.value); setOpen(true); }}
          onFocus={() => setOpen(true)}
          onKeyDown={handleKeyDown}
          placeholder={value.length ? '' : placeholder}
          className="flex-1 min-w-[120px] bg-transparent outline-none text-amber-50 placeholder-amber-300/60"
        />
      </div>
      {open && (suggestions.length > 0 || (input.trim() && !suggestions.some((s) => s.name.toLowerCase() === input.trim().toLowerCase()))) && (
        <ul className="absolute top-full left-0 right-0 mt-1 max-h-48 overflow-y-auto rounded-xl bg-amber-900 border border-amber-700/50 shadow-xl z-10">
          {suggestions.map((s) => (
            <li key={s.id}>
              <button
                type="button"
                className="w-full text-left px-4 py-2 hover:bg-amber-800/80 text-amber-100"
                onClick={() => addIngredient({ id: s.id, name: s.name })}
              >
                {s.name}
              </button>
            </li>
          ))}
          {input.trim() && (
            <li>
              <button
                type="button"
                className="w-full text-left px-4 py-2 hover:bg-amber-800/80 text-amber-200 italic"
                onClick={() => addIngredient({ id: null, name: input.trim() })}
              >
                Add &quot;{input.trim()}&quot; as free text
              </button>
            </li>
          )}
        </ul>
      )}
    </div>
  );
}
