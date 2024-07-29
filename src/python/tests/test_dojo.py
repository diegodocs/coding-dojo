import app.dojo
import pytest

def test_when_conditions_then_results():    
    # arrange
    expectedValue = True
    currentValue = False

    # act
    currentValue = app.dojo.calculate()

    # assert
    assert currentValue == expectedValue