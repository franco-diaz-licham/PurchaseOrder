const money = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD'
});

export const formatMoney = (value: number) => money.format(value);
