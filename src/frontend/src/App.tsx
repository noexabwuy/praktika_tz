import { useState } from 'react';
import { Search } from 'lucide-react';

function App() {
  const [checked, setChecked] = useState(false);

  return (
    <div className="min-h-screen bg-[#e5e7eb] p-6 flex justify-center font-sans">
      <div className="max-w-[1100px] w-full flex flex-col gap-10">
        {/* Primary + Secondary + Search */}
        <div className="flex flex-wrap gap-8 bg-white p-6 rounded-xl shadow-sm">
          <div className="flex flex-col gap-2 flex-1 min-w-[140px]">
            <h2 className="text-sm font-semibold text-[#4b5563] mb-1">Primary button</h2>
            <button className="w-[200px] h-[42px] bg-[#10b981] text-white font-semibold text-base hover:bg-[#087A54] active:bg-[#065f46] disabled:bg-[#9ca3af] disabled:opacity-60 transition-colors flex items-center justify-center">Button</button>
            <button className="w-[200px] h-[42px] bg-[#10b981] text-white font-semibold text-base hover:bg-[#087A54] active:bg-[#065f46] disabled:bg-[#9ca3af] disabled:opacity-60 transition-colors flex items-center justify-center">Button</button>
            <button className="w-[200px] h-[42px] bg-[#10b981] text-white font-semibold text-base hover:bg-[#087A54] active:bg-[#065f46] disabled:bg-[#9ca3af] disabled:opacity-60 transition-colors flex items-center justify-center">Button</button>
          </div>

          <div className="flex flex-col gap-2 flex-1 min-w-[140px]">
            <h2 className="text-sm font-semibold text-[#4b5563] mb-1">Secondary button</h2>
            <button className="w-[200px] h-[42px] bg-white text-[#111827] font-semibold text-base border border-[#d1d5db] hover:bg-[#f9fafb] hover:border-[#10b981] hover:text-[#10b981] active:bg-[#e5e7eb] active:border-[#087A54] active:text-[#087A54] disabled:bg-[#f3f4f6] disabled:border-[#d1d5db] disabled:text-[#9ca3af] transition-colors flex items-center justify-center">Button</button>
            <button className="w-[200px] h-[42px] bg-white text-[#111827] font-semibold text-base border border-[#d1d5db] hover:bg-[#f9fafb] hover:border-[#10b981] hover:text-[#10b981] active:bg-[#e5e7eb] active:border-[#087A54] active:text-[#087A54] disabled:bg-[#f3f4f6] disabled:border-[#d1d5db] disabled:text-[#9ca3af] transition-colors flex items-center justify-center">Button</button>
            <button className="w-[200px] h-[42px] bg-white text-[#111827] font-semibold text-base border border-[#d1d5db] hover:bg-[#f9fafb] hover:border-[#10b981] hover:text-[#10b981] active:bg-[#e5e7eb] active:border-[#087A54] active:text-[#087A54] disabled:bg-[#f3f4f6] disabled:border-[#d1d5db] disabled:text-[#9ca3af] transition-colors flex items-center justify-center">Button</button>
          </div>

          <div className="flex flex-col gap-2 flex-[1.5] min-w-[200px]">
            <h2 className="text-sm font-semibold text-[#4b5563] mb-1">Search</h2>
            <div className="relative w-[280px] h-[42px] group">
              <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-[18px] h-[18px] text-[#4b5563] group-hover:text-[#10b981] pointer-events-none transition-colors" />
              <input
                type="text"
                placeholder="Введите запрос..."
                className="w-full h-full pl-9 pr-4 border border-[#d1d5db] bg-white text-[#111827] placeholder:text-[#4b5563] outline-none transition-all group-hover:border-[#10b981] group-hover:bg-[#f9fafb] focus:border-[#10b981] focus:ring-2 focus:ring-[#10b981] focus:ring-opacity-20"
              />
            </div>
          </div>
        </div>

        {/* Icon buttons */}
        <div className="bg-white p-6 rounded-xl shadow-sm">
          <h2 className="text-sm font-semibold text-[#4b5563] mb-4">Icon buttons</h2>
          <div className="flex flex-wrap gap-6 items-end">
            {/* Close (×) */}
            <div className="flex flex-col items-center gap-2">
              <button className="w-[42px] h-[42px] flex items-center justify-center border border-[#d1d5db] bg-white text-[#4b5563] hover:bg-[#f9fafb] hover:border-[#10b981] hover:text-[#10b981] transition-all text-2xl font-mono">×</button>
              <span className="text-xs text-[#6b7280]">Close</span>
            </div>
            {/* Add (+) */}
            <div className="flex flex-col items-center gap-2">
              <button className="w-[42px] h-[42px] flex items-center justify-center border border-[#d1d5db] bg-white text-[#4b5563] hover:bg-[#f9fafb] hover:border-[#10b981] hover:text-[#10b981] transition-all text-2xl font-mono">+</button>
              <span className="text-xs text-[#6b7280]">Add</span>
            </div>
            {/* Filter 1 (серый) */}
            <div className="flex flex-col items-center gap-2">
              <button className="w-[42px] h-[42px] flex items-center justify-center border border-[#d1d5db] bg-white text-[#4b5563] hover:bg-[#f9fafb] hover:border-[#10b981] hover:text-[#10b981] transition-all text-2xl font-mono">⏚</button>
              <span className="text-xs text-[#6b7280]">Filter (gray)</span>
            </div>
            {/* Filter 2 (primary) */}
            <div className="flex flex-col items-center gap-2">
              <button className="w-[42px] h-[42px] flex items-center justify-center border border-[#10b981] bg-white text-[#10b981] hover:bg-[#f9fafb] hover:border-[#087A54] hover:text-[#087A54] transition-all text-2xl font-mono">⏚</button>
              <span className="text-xs text-[#6b7280]">Filter (primary)</span>
            </div>
            {/* Checkbox (интерактивный) */}
            <div className="flex flex-col items-center gap-2">
              <button
                className={`w-[42px] h-[42px] flex items-center justify-center border transition-all text-2xl font-mono ${
                  checked
                    ? 'bg-[#10b981] border-[#10b981] text-white'
                    : 'bg-white border-[#d1d5db] text-transparent hover:border-[#10b981]'
                }`}
                onClick={() => setChecked(!checked)}
              >
                {checked ? '✓' : ''}
              </button>
              <span className="text-xs text-[#6b7280]">Checkbox</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;
