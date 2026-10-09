// AdminLTE si aspetta le classi di layout sul <body>: qui il contenitore è #root.
export const addWindowClass = (className: string) => {
  document.getElementById('root')?.classList.add(className);
};

export const removeWindowClass = (className: string) => {
  document.getElementById('root')?.classList.remove(className);
};
