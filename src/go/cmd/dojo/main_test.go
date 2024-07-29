package main

import "testing"

func TestCalculate(t *testing.T) {

	// arrange
	expectedValue := true
	currentValue := false

	//act
	currentValue = Calculate()

	//assert
	if expectedValue != currentValue {
		t.Errorf("Expected: %t, Got: %t", expectedValue, currentValue)
	}

}
