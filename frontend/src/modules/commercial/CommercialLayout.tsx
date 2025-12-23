import { Outlet } from 'react-router-dom';

export function CommercialLayout() {
  return (
    <div className="flex flex-col h-full">
      {/* Aqui podemos adicionar um sub-header ou breadcrumbs específicos do módulo */}
      <div className="flex-1 overflow-auto">
        <Outlet />
      </div>
    </div>
  );
}
