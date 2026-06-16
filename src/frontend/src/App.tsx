import { useState } from 'react';
import { X, Plus, Minus, Check, Filter, Search } from 'lucide-react';
import './App.css';

function App() {
  const [activeFilters, setActiveFilters] = useState<string[]>([]);

  const toggleFilter = (id: string) => {
    setActiveFilters((prev) =>
      prev.includes(id) ? prev.filter((f) => f !== id) : [...prev, id]
    );
  };

  return (
    <div className="container">
      {/* 1. Primary и Secondary кнопки */}
      <section className="section">
        <div className="row">
          <div className="col">
            <h2 className="section-title">Primary button</h2>
            <button className="btn-primary">Default</button>
            <button className="btn-primary">Hover (наведи)</button>
            <button className="btn-primary">Pressed (зажми)</button>
            <button className="btn-primary" disabled>Disabled</button>
          </div>
          <div className="col">
            <h2 className="section-title">Secondary button</h2>
            <button className="btn-secondary">Default</button>
            <button className="btn-secondary">Hover</button>
            <button className="btn-secondary">Pressed</button>
            <button className="btn-secondary" disabled>Disabled</button>
          </div>
        </div>
      </section>

      {/* 2.Поисковая строка */}
      <section className="section">
        <h2 className="section-title">Search (600px)</h2>
        <div className="search-group">
          <div className="search-large">
            <Search className="icon" size={20} />
            <input type="text" placeholder="Введите запрос..." />
          </div>
          <div className="search-large">
            <Search className="icon" size={20} />
            <input type="text" placeholder="Disabled" disabled />
          </div>
        </div>
      </section>

      {/* 3. Иконки  */}
      <section className="section">
        <h2 className="section-title">Icon buttons (Lucide)</h2>
        <div className="grid-5">
          <button className="icon-btn green"><X size={20} /></button>
          <button className="icon-btn green"><Plus size={20} /></button>
          <button className="icon-btn green"><Filter size={20} /></button>
          <button className="icon-btn green"><Filter size={20} /></button>
          <div className="placeholder"><div className="dot"></div></div>
        </div>
        <div className="disabled-demo">
          <span className="label">Disabled:</span>
          <button className="icon-btn green" disabled><X size={20} /></button>
          <button className="icon-btn white" disabled><Plus size={20} /></button>
        </div>
      </section>

      {/*4. Текстовые символы  */}
      <section className="section">
        <h2 className="section-title">Symbol buttons (Lucide)</h2>
        <div className="grid-5">
          <button className="symbol-btn"><Plus size={20} /></button>
          <button className="symbol-btn"><Minus size={20} /></button>
          <button className="symbol-btn"><Check size={20} /></button>
          <button className="symbol-btn"><X size={20} /></button>
          <button className="symbol-btn white"><div className="dot"></div></button>
        </div>
        <div className="disabled-demo">
          <span className="label">Disabled:</span>
          <button className="symbol-btn" disabled><Plus size={20} /></button>
          <button className="symbol-btn white" disabled><Check size={20} /></button>
        </div>
      </section>

      {/* 5. Фильтры-тогглы */}
      <section className="section">
        <h2 className="section-title">Filter toggles (click to toggle)</h2>
        <div className="grid-5">
          <button
            className={`filter-btn ${activeFilters.includes('remove') ? 'active' : ''}`}
            onClick={() => toggleFilter('remove')}
          >
            <X size={20} />
          </button>
          <button
            className={`filter-btn ${activeFilters.includes('add') ? 'active' : ''}`}
            onClick={() => toggleFilter('add')}
          >
            <Plus size={20} />
          </button>
          <button
            className={`filter-btn ${activeFilters.includes('filter1') ? 'active' : ''}`}
            onClick={() => toggleFilter('filter1')}
          >
            <Filter size={20} />
          </button>
          <button
            className={`filter-btn ${activeFilters.includes('filter2') ? 'active' : ''}`}
            onClick={() => toggleFilter('filter2')}
          >
            <Filter size={20} />
          </button>
          <div className="placeholder"><div className="dot"></div></div>
        </div>
        <div className="disabled-demo">
          <span className="label">Disabled:</span>
          <button className="filter-btn" disabled><X size={20} /></button>
        </div>
      </section>
    </div>
  );
}

export default App;
