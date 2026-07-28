import { Welcom } from './welcom';

describe('Welcom', () => {
  let component: Welcom;

  beforeEach(() => {
    component = new Welcom();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
